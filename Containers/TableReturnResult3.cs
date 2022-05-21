using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechpoolUnleashed.Collections
{
    public class TableReturnResult3
    {
        public readonly string UID;
        public readonly string Name;
        public readonly string PUID;

        public TableReturnResult3(string uid, string name)
        {
            this.UID = uid;
            this.Name = name;
            this.PUID = "";
        }
        public TableReturnResult3(string uid, string name, string puid)
        {
            this.UID = uid;
            this.Name = name;
            this.PUID = puid;
        }

        public string ToString()
        {
            return $"UID: {UID}, Name: {Name}, PUID: {PUID}";
        }
    }
}
