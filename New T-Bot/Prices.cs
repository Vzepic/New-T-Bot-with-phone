using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace New_T_Bot
{
    class Prices
    {
        public double PriceA;
        public double PriceB;
        public double PriceAAwins;
        public double PriceBAwins;
        public double PriceABwins;
        public double PriceBBwins;


        public Prices()
        {
        }

        public Prices(double _PriceA, double _PriceB, double _PriceAAwins, double _PriceBAwins, double _PriceABwins, double _PriceBBwins)
        {
            this.PriceA = _PriceA;
            this.PriceB = _PriceB;
            this.PriceAAwins = _PriceAAwins;
            this.PriceBAwins = _PriceBAwins;
            this.PriceABwins = _PriceABwins;
            this.PriceBBwins = _PriceBBwins;
        }

    }


}
