using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace New_T_Bot
{
    class Player
    {
        public string Name;
        public int selectionID;
        public string OurDBName;

        public Player()
        {
        }

        public Player(string _Name, int _selectionID, string _OurDBName)
        {
            this.Name = _Name;
            this.selectionID = _selectionID;
            this.OurDBName = _OurDBName;
        }
    }
}
