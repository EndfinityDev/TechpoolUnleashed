using System;
using System.Collections.Generic;
using System.Data.SQLite;
using TechpoolUnleashed.Collections;

namespace TechpoolUnleashed.SQLite
{
    public class DatabaseManager
    {
        SQLiteConnection _connection;

        string _dbPath;

        bool _isDBForcedOpen = false;

        // Name lookup helper
        SQLiteCommand _selectUIDNameFromFamiliesWhereNameLike;
        SQLiteCommand _selectUIDNameFUIDFromVariantsWhereNameLike;
        SQLiteCommand _selectUIDNameFromModelsWhereNameLike;
        SQLiteCommand _selectUIDNameMUIDFromTrimsWhereNameLike;

        // Parent name lookup by child UID
        SQLiteCommand _selectNameFromFamililiesWhereUIDIs;
        SQLiteCommand _selectNameFromModelsWhereUIDIs;

        // Children name lookup by parent UID
        SQLiteCommand _selectNameFromVariantsWhereFUIDIs;
        SQLiteCommand _selectNameFromTrimsWhereMUIDIs;

        SQLiteCommand _selectNameFromTrimsWhereVUIDIs;

        // Picking UIDs
        //SQLiteCommand _selectUIDFromFamiliesWhereNameIs;
        //SQLiteCommand _selectUIDFromVariantsWhereNameIs;
        //SQLiteCommand _selectUIDFromModelsWhereNameIs;
        //SQLiteCommand _selectUIDFromTrimsWhereNameIs;

        // Picking techpool by UID
        SQLiteCommand _selectTechPoolFromFamiliesWhereUIDIs;
        SQLiteCommand _selectTechPoolFromVariantsWhereUIDIs;
        SQLiteCommand _selectTechPoolFromModelsWhereUIDIs;
        SQLiteCommand _selectTechPoolFromTrimsWhereUIDIs;

        // Updating Techpool
        SQLiteCommand _updateFamiliesSetTechpoolWhereUIDIs;
        SQLiteCommand _updateVariantsSetTechpoolWhereUIDIs;
        SQLiteCommand _updateModelsSetTechpoolWhereUIDIs;
        SQLiteCommand _updateTrimsSetTechpoolWhereUIDIs;

        // Parameter used in every command, set for each before execution
        SQLiteParameter _commandParameter;

        // Parameter for setting techpool
        SQLiteParameter _techpoolParameter;


        public DatabaseManager(string dbpath)
        {
            // Preparing connection information
            // The connection does NOT open here
            _dbPath = dbpath;
            _connection = new SQLiteConnection("Data Source="+_dbPath);

            RunTestCommand();

            // Creating all commands
            _selectUIDNameFromFamiliesWhereNameLike = _connection.CreateCommand();
            _selectUIDNameFromFamiliesWhereNameLike.CommandText =
                "SELECT UID, Name FROM Families WHERE Name LIKE $param";

            _selectUIDNameFUIDFromVariantsWhereNameLike = _connection.CreateCommand();
            _selectUIDNameFUIDFromVariantsWhereNameLike.CommandText =
                "SELECT UID, Name, FUID FROM Variants WHERE Name LIKE $param";

            _selectUIDNameFromModelsWhereNameLike = _connection.CreateCommand();
            _selectUIDNameFromModelsWhereNameLike.CommandText =
                "SELECT UID, Name FROM Models WHERE Name LIKE $param";

            _selectUIDNameMUIDFromTrimsWhereNameLike = _connection.CreateCommand();
            _selectUIDNameMUIDFromTrimsWhereNameLike.CommandText =
                "SELECT UID, Name, MUID FROM Trims WHERE Name LIKE $param";

            _selectNameFromFamililiesWhereUIDIs = _connection.CreateCommand();
            _selectNameFromFamililiesWhereUIDIs.CommandText =
                "SELECT Name FROM Families WHERE UID = $param";

            _selectNameFromModelsWhereUIDIs = _connection.CreateCommand();
            _selectNameFromModelsWhereUIDIs.CommandText =
                "SELECT Name FROM Models WHERE UID = $param";

            _selectNameFromVariantsWhereFUIDIs = _connection.CreateCommand();
            _selectNameFromVariantsWhereFUIDIs.CommandText =
                "SELECT Name FROM Variants WHERE FUID = $param";

            _selectNameFromTrimsWhereMUIDIs = _connection.CreateCommand();
            _selectNameFromTrimsWhereMUIDIs.CommandText =
                "SELECT Name FROM Trims WHERE MUID = $param";

            _selectNameFromTrimsWhereVUIDIs = _connection.CreateCommand();
            _selectNameFromTrimsWhereVUIDIs.CommandText =
                "SELECT Name FROM Trims WHERE VUID = $param";

            _selectTechPoolFromFamiliesWhereUIDIs = _connection.CreateCommand();
            _selectTechPoolFromFamiliesWhereUIDIs.CommandText =
                "SELECT TechPool FROM Families WHERE UID = $param";

            _selectTechPoolFromVariantsWhereUIDIs = _connection.CreateCommand();
            _selectTechPoolFromVariantsWhereUIDIs.CommandText =
                "SELECT TechPool FROM Variants WHERE UID = $param";

            _selectTechPoolFromModelsWhereUIDIs = _connection.CreateCommand();
            _selectTechPoolFromModelsWhereUIDIs.CommandText =
                "SELECT TechPool FROM Models WHERE UID = $param";

            _selectTechPoolFromTrimsWhereUIDIs = _connection.CreateCommand();
            _selectTechPoolFromTrimsWhereUIDIs.CommandText =
                "SELECT TechPool FROM Trims WHERE UID = $param";

            _updateFamiliesSetTechpoolWhereUIDIs = _connection.CreateCommand();
            _updateFamiliesSetTechpoolWhereUIDIs.CommandText =
                "UPDATE Families SET TechPool = $techpool WHERE UID = $param";

            _updateVariantsSetTechpoolWhereUIDIs = _connection.CreateCommand();
            _updateVariantsSetTechpoolWhereUIDIs.CommandText =
                "UPDATE Variants SET TechPool = $techpool WHERE UID = $param";

            _updateModelsSetTechpoolWhereUIDIs = _connection.CreateCommand();
            _updateModelsSetTechpoolWhereUIDIs.CommandText =
                "UPDATE Models SET TechPool = $techpool WHERE UID = $param";

            _updateTrimsSetTechpoolWhereUIDIs = _connection.CreateCommand();
            _updateTrimsSetTechpoolWhereUIDIs.CommandText =
                "UPDATE Trims SET TechPool = $techpool WHERE UID = $param";

            // Applying the parameter to all commands
            // the parameter value is set before execution rather than here
            _commandParameter = new SQLiteParameter("$param");
            _techpoolParameter = new SQLiteParameter("$techpool");

            _selectUIDNameFromFamiliesWhereNameLike.Parameters.Add(_commandParameter);
            _selectUIDNameFUIDFromVariantsWhereNameLike.Parameters.Add(_commandParameter);
            _selectUIDNameFromModelsWhereNameLike.Parameters.Add(_commandParameter);
            _selectUIDNameMUIDFromTrimsWhereNameLike.Parameters.Add(_commandParameter);
            _selectNameFromFamililiesWhereUIDIs.Parameters.Add(_commandParameter);
            _selectNameFromModelsWhereUIDIs.Parameters.Add(_commandParameter);
            _selectNameFromVariantsWhereFUIDIs.Parameters.Add(_commandParameter);
            _selectNameFromTrimsWhereMUIDIs.Parameters.Add(_commandParameter);
            _selectNameFromTrimsWhereVUIDIs.Parameters.Add(_commandParameter);
            _selectTechPoolFromFamiliesWhereUIDIs.Parameters.Add(_commandParameter);
            _selectTechPoolFromVariantsWhereUIDIs.Parameters.Add(_commandParameter);
            _selectTechPoolFromModelsWhereUIDIs.Parameters.Add(_commandParameter);
            _selectTechPoolFromTrimsWhereUIDIs.Parameters.Add(_commandParameter);

            _updateFamiliesSetTechpoolWhereUIDIs.Parameters.Add(_commandParameter);
            _updateVariantsSetTechpoolWhereUIDIs.Parameters.Add(_commandParameter);
            _updateModelsSetTechpoolWhereUIDIs.Parameters.Add(_commandParameter);
            _updateTrimsSetTechpoolWhereUIDIs.Parameters.Add(_commandParameter);

            _updateFamiliesSetTechpoolWhereUIDIs.Parameters.Add(_techpoolParameter);
            _updateVariantsSetTechpoolWhereUIDIs.Parameters.Add(_techpoolParameter);
            _updateModelsSetTechpoolWhereUIDIs.Parameters.Add(_techpoolParameter);
            _updateTrimsSetTechpoolWhereUIDIs.Parameters.Add(_techpoolParameter);
        }

        public void ForceOpenDatabase()
        {
            _isDBForcedOpen = true;
            _connection.Open();
        }
        public void ForceCloseDatabase()
        {
            _connection.Close();
            _isDBForcedOpen = false;
        }

        public TableReturnResult3[] FindFamiliesLike(string name)
        {
            _commandParameter.Value = name;
            if (!_isDBForcedOpen) _connection.Open();
            SQLiteDataReader resultReader = _selectUIDNameFromFamiliesWhereNameLike.ExecuteReader();

            string uidres, nameres;
            List<TableReturnResult3> result = new List<TableReturnResult3>();
            while (resultReader.Read())
            {
                uidres = resultReader.GetString(0);
                nameres = resultReader.GetString(1);
                result.Add(new TableReturnResult3(uidres, nameres));
            }
            resultReader.Close();

            if (!_isDBForcedOpen) _connection.Close();

            return result.ToArray();
        }
        public TableReturnResult3[] FindVariantsLike(string name)
        {
            _commandParameter.Value = name;
            if (!_isDBForcedOpen) _connection.Open();
            SQLiteDataReader resultReader = _selectUIDNameFUIDFromVariantsWhereNameLike.ExecuteReader();

            string uidres, nameres, fuidres;
            List<TableReturnResult3> result = new List<TableReturnResult3>();
            while (resultReader.Read())
            {
                uidres = resultReader.GetString(0);
                nameres = resultReader.GetString(1);
                fuidres = resultReader.GetString(2);
                result.Add(new TableReturnResult3(uidres, nameres, fuidres));
            }
            resultReader.Close();

            if (!_isDBForcedOpen) _connection.Close();

            return result.ToArray();
        }
        public TableReturnResult3[] FindModelsLike(string name)
        {
            _commandParameter.Value = name;
            if (!_isDBForcedOpen) _connection.Open();
            SQLiteDataReader resultReader = _selectUIDNameFromModelsWhereNameLike.ExecuteReader();

            string uidres, nameres;
            List<TableReturnResult3> result = new List<TableReturnResult3>();
            while (resultReader.Read())
            {
                uidres = resultReader.GetString(0);
                nameres = resultReader.GetString(1);
                result.Add(new TableReturnResult3(uidres, nameres));
            }
            resultReader.Close();

            if (!_isDBForcedOpen) _connection.Close();

            return result.ToArray();
        }
        public TableReturnResult3[] FindTrimsLike(string name)
        {
            _commandParameter.Value = name;
            if (!_isDBForcedOpen) _connection.Open();
            SQLiteDataReader resultReader = _selectUIDNameMUIDFromTrimsWhereNameLike.ExecuteReader();

            string uidres, nameres, muidres;
            List<TableReturnResult3> result = new List<TableReturnResult3>();
            while (resultReader.Read())
            {
                uidres = resultReader.GetString(0);
                nameres = resultReader.GetString(1);
                muidres = resultReader.GetString(2);
                result.Add(new TableReturnResult3(uidres, nameres, muidres));
            }
            resultReader.Close();

            if (!_isDBForcedOpen) _connection.Close();

            return result.ToArray();
        }

        public string FindFamilyName(string uid)
        {
            _commandParameter.Value = uid;
            if (!_isDBForcedOpen) _connection.Open();
            
            string result = (string)_selectNameFromFamililiesWhereUIDIs.ExecuteScalar();

            if (!_isDBForcedOpen) _connection.Close();

            return result;
        }
        public string FindModelName(string uid)
        {
            _commandParameter.Value = uid;
            if (!_isDBForcedOpen) _connection.Open();

            string result = (string)_selectNameFromModelsWhereUIDIs.ExecuteScalar();

            if (!_isDBForcedOpen) _connection.Close();

            return result;
        }

        public string[] FindVariantNamesByFUID(string fuid)
        {
            _commandParameter.Value = fuid;
            if (!_isDBForcedOpen) _connection.Open();
            SQLiteDataReader resultReader = _selectNameFromVariantsWhereFUIDIs.ExecuteReader();

            List<string> result = new List<string>();
            while (resultReader.Read())
            {
                result.Add(resultReader.GetString(0));
            }
            resultReader.Close();

            if (!_isDBForcedOpen) _connection.Close();

            return result.ToArray();
        }
        public string[] FindTrimNamesByMUID(string muid)
        {
            _commandParameter.Value = muid;
            if (!_isDBForcedOpen) _connection.Open();
            SQLiteDataReader resultReader = _selectNameFromTrimsWhereMUIDIs.ExecuteReader();

            List<string> result = new List<string>();
            while (resultReader.Read())
            {
                result.Add(resultReader.GetString(0));
            }
            resultReader.Close();

            if (!_isDBForcedOpen) _connection.Close();

            return result.ToArray();
        }
        public string[] FindTrimNamesByVUID(string vuid)
        {
            _commandParameter.Value = vuid;
            if (!_isDBForcedOpen) _connection.Open();
            SQLiteDataReader resultReader = _selectNameFromTrimsWhereVUIDIs.ExecuteReader();

            List<string> result = new List<string>();
            while (resultReader.Read())
            {
                result.Add(resultReader.GetString(0));
            }
            resultReader.Close();

            if (!_isDBForcedOpen) _connection.Close();

            return result.ToArray();
        }
        public byte[] GetFamilyTechpool(string uid)
        {
            _commandParameter.Value = uid;
            if (!_isDBForcedOpen) _connection.Open();
            byte[] result = (byte[])_selectTechPoolFromFamiliesWhereUIDIs.ExecuteScalar();


            if (!_isDBForcedOpen) _connection.Close();

            return result;
        }
        public byte[] GetVariantTechpool(string uid)
        {
            _commandParameter.Value = uid;
            if (!_isDBForcedOpen) _connection.Open();
            byte[] result = (byte[])_selectTechPoolFromVariantsWhereUIDIs.ExecuteScalar();


            if (!_isDBForcedOpen) _connection.Close();

            return result;
        }
        public byte[] GetModelTechpool(string uid)
        {
            _commandParameter.Value = uid;
            if (!_isDBForcedOpen) _connection.Open();
            byte[] result = (byte[])_selectTechPoolFromModelsWhereUIDIs.ExecuteScalar();


            if (!_isDBForcedOpen) _connection.Close();

            return result;
        }
        public byte[] GetTrimTechpool(string uid)
        {
            _commandParameter.Value = uid;
            if (!_isDBForcedOpen) _connection.Open();
            byte[] result = (byte[])_selectTechPoolFromTrimsWhereUIDIs.ExecuteScalar();


            if (!_isDBForcedOpen) _connection.Close();

            return result;
        }

        public void SetFamilyTechpool(string uid, byte[] techpool)
        {
            _commandParameter.Value = uid;
            _techpoolParameter.Value = techpool;

            if (!_isDBForcedOpen) _connection.Open();

            _updateFamiliesSetTechpoolWhereUIDIs.ExecuteNonQuery();
            
            if (!_isDBForcedOpen) _connection.Close();
            return;
        }
        public void SetVariantTechpool(string uid, byte[] techpool)
        {
            _commandParameter.Value = uid;
            _techpoolParameter.Value = techpool;

            if (!_isDBForcedOpen) _connection.Open();

            _updateVariantsSetTechpoolWhereUIDIs.ExecuteNonQuery();

            if (!_isDBForcedOpen) _connection.Close();
            return;
        }
        public void SetModelTechpool(string uid, byte[] techpool)
        {
            _commandParameter.Value = uid;
            _techpoolParameter.Value = techpool;

            if (!_isDBForcedOpen) _connection.Open();

            _updateModelsSetTechpoolWhereUIDIs.ExecuteNonQuery();

            if (!_isDBForcedOpen) _connection.Close();
            return;
        }
        public void SetTrimTechpool(string uid, byte[] techpool)
        {
            _commandParameter.Value = uid;
            _techpoolParameter.Value = techpool;

            if (!_isDBForcedOpen) _connection.Open();

            _updateTrimsSetTechpoolWhereUIDIs.ExecuteNonQuery();

            if (!_isDBForcedOpen) _connection.Close();
            return;
        }

        private void RunTestCommand()
        {
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT Name FROM Families";
            _connection.Open();
            command.ExecuteNonQuery();
            _connection.Close();
        }

        public string[] Test(string name)
        {
            // Setting the parameter before execution
            _commandParameter.Value = name;

            var command = _connection.CreateCommand();
            command.CommandText =
                "SELECT UID, Name FROM Trims WHERE Name LIKE $param";
            command.Parameters.Add(_commandParameter);

            _connection.Open();

            var resultReader = command.ExecuteReader();
            List<string> result = new List<string>();
            while (resultReader.Read())
            {
                result.Add(resultReader.GetString(0));
                result.Add(resultReader.GetString(1));
            }

            _connection.Close();

            return result.ToArray();
        }
    }
}
