using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace TechpoolUnleashed
{
    class DeserializedItemTable
    {
        public Hashtable ContentTable = new Hashtable();
        Stack<Hashtable> _hashtablesStack = new Stack<Hashtable>{};
        Stack<Int32> _tableItemsRemaining = new Stack<Int32>();

        byte[] _sourceBytes;
        Int32 _cursor = 0;
        

        bool _awaitParameterName = false;
        string _parameterName = "";

        Int32 _indent = 0;

        string _defaultTableName = "";


        public DeserializedItemTable(byte[] bytes)
        {
            _sourceBytes = bytes;
            //_hashtablesStack.Push(_contentTable);
            //_tableItemsRemaining.Push(Int32.MaxValue);

            Int32 byteLen = _sourceBytes.Length;
            while (_cursor < byteLen) if(!ReadNextValue()) throw new Exception("Could not process value");
        }
        public DeserializedItemTable(byte[] bytes, string defautTableName)
        {
            _sourceBytes = bytes;
            //_hashtablesStack.Push(_contentTable);
            //_tableItemsRemaining.Push(Int32.MaxValue);
            _defaultTableName = defautTableName;

            Int32 byteLen = _sourceBytes.Length;
            while (_cursor < byteLen) if (!ReadNextValue()) throw new Exception("Could not process value");
        }

        public Hashtable GetTable()
        {
            return ContentTable;
        }
        public string GetTableString()
        {
            return UnwrapHashtable(ContentTable, _defaultTableName)+ "}\n";
        }
        string UnwrapHashtable(Hashtable table, string name)
        {


            StringBuilder hashtableString = new StringBuilder();
            for (int i = 0; i < _indent; i++) hashtableString.Append("\t");
            //if (name == "") hashtableString.Append("{\n");
            hashtableString.Append("\"").Append(name).Append("\"").Append(": {\n");
            _indent++;
            Int32 tableLength = table.Count;
            Int32 counter = 0;

            foreach (var key in table.Keys)
            {
                counter++;
                if(table[key].GetType() == typeof(Hashtable))
                {
                    //_indent++;
                    hashtableString.Append(UnwrapHashtable((Hashtable)table[key], (string)key));
                }
                else
                {
                    for (int i = 0; i < _indent; i++) hashtableString.Append("\t");
                    hashtableString.Append("\"").Append(key).Append("\"").Append(": ");
                    bool isStr = table[key].GetType() == typeof(string);
                    if (isStr) hashtableString.Append("\"");
                    hashtableString.Append(table[key]);
                    if (isStr) hashtableString.Append("\"");
                    if (counter != tableLength) hashtableString.Append(",");
                    hashtableString.Append("\n");
                }
            }
            for (int i = 0; i < _indent; i++) hashtableString.Append("\t");
            //if (_indent == 0) hashtableString.Append("}\n");
            //else 
            hashtableString.Append("},\n");
            _indent--;
            return hashtableString.ToString();
        }

        public byte[] Serialize()
        {
            return SerializeHashTable(ContentTable, _defaultTableName);
        }
        byte[] SerializeHashTable(Hashtable table, string name)
        {
            List<byte> bytes = new List<byte>();

            byte[] tempBytes;

            tempBytes = Serializer.BuildTable(table.Count, name);

            foreach(byte b in tempBytes) 
                bytes.Add(b);

            foreach (var key in table.Keys)
            {
                //Console.WriteLine(table[key].GetType());
                if (table[key].GetType() == typeof(Hashtable))
                {
                    tempBytes = SerializeHashTable((Hashtable)table[key], (string)key);
                }

                if(table[key].GetType() == typeof(string))
                {
                    tempBytes = Serializer.BuildString((string)table[key], (string)key);
                }
                if (table[key].GetType() == typeof(double))
                {
                    tempBytes = Serializer.BuildNumber((double)table[key], (string)key);
                }
                
                foreach (byte b in tempBytes) bytes.Add(b);
            }

            return bytes.ToArray();
        }

        bool ReadNextValue()
        {
            //Console.WriteLine(_sourceBytes[_cursor]);

            if(Encoding.UTF8.GetString(_sourceBytes,_cursor,1) == "T")
            {
                _cursor += 1;
                if (!HandleTableItem()) return false;
            }
            else if(Encoding.UTF8.GetString(_sourceBytes,_cursor,1) == "S")
            {
                _cursor++;
                if (!HandleStringItem()) return false;
            }
            else if (Encoding.UTF8.GetString(_sourceBytes, _cursor, 1) == "N")
            {
                _cursor++;
                if (!HandleNumberItem()) return false;
            }
            else
            {
                //Console.WriteLine("Unknown data type " + _sourceBytes[_cursor] + ", skipping...");
                _cursor++;
                //return false;
            }
            return true;
        }
        bool HandleTableItem()
        {
            _cursor += sizeof(Int32); //Unknown data, ignoring
            Int32 tableSize = BitConverter.ToInt32(_sourceBytes, _cursor);
            _cursor += sizeof(Int32);
            Hashtable ht;
            Hashtable newTable;
            if (_hashtablesStack.Count != 0)
            {
                ht = _hashtablesStack.Pop();

                newTable = new Hashtable();
                ht.Add(_parameterName, newTable);
                _hashtablesStack.Push(ht);
            }
            else
            {
                ContentTable = new Hashtable();
                newTable = ContentTable;
                ht = newTable;
            }
            _hashtablesStack.Push(newTable);
            _tableItemsRemaining.Push(tableSize);
            _awaitParameterName = true;
            _parameterName = "";
            return true;
        }
        bool HandleStringItem()
        {
            Int32 strLength = BitConverter.ToInt32(_sourceBytes, _cursor);
            _cursor += sizeof(Int32);
            string str = Encoding.UTF8.GetString(_sourceBytes, _cursor, strLength);
            _cursor += strLength;
            if(_awaitParameterName)
            {
                _parameterName = str;
                _awaitParameterName = false;
                return true;
            }
            else
            {
                
                Hashtable ht = _hashtablesStack.Peek();
                /*if (_parameterName == "Fixtures")
                {
                    List<byte> bytes = new List<byte>();
                    for (int i = _cursor - strLength; i < _cursor; i++)
                    {
                        bytes.Add(_sourceBytes[i]);
                    }
                    Hashtable dt = new DeserializedItemTable(bytes.ToArray(), _parameterName).GetTable();
                    ht.Add(_parameterName, dt);
                    _hashtablesStack.Push(dt);
                    _tableItemsRemaining.Push(0);
                    HandleStackUpdate();
                }
                else*/ 
                ht.Add(_parameterName, str);
                
                _parameterName = "";
                _awaitParameterName = true;
                HandleStackUpdate();
                return true;
            }
        }
        bool HandleNumberItem()
        {
            double num = BitConverter.ToDouble(_sourceBytes, _cursor);
            _cursor += sizeof(double);
            Hashtable ht = _hashtablesStack.Peek();
            ht.Add(_parameterName, num);
            _parameterName = "";
            _awaitParameterName = true;
            HandleStackUpdate();
            return true;
        }
        void HandleStackUpdate()
        {
            if (_tableItemsRemaining.Count == 0) return;
            int tableLen = _tableItemsRemaining.Pop();
            tableLen--;
            if (tableLen > 0) _tableItemsRemaining.Push(tableLen);
            else
            {
                _hashtablesStack.Pop();
                //_indent--;
                HandleStackUpdate();
            }
        }
    }

}
