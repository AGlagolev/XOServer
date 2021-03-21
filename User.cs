using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XOServer
{
    class User
    {
        private string _ip;
        private string _name;

        public User(string ip, string name)
        {
            _ip = ip;
            _name = name;
        }

        public string IP
        {
            get { return _ip; }
        } 
        public string Name
        {
            get { return _name; }
        }
    }
}
