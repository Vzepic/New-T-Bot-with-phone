using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace New_T_Bot
{
    class Market
    {
        public List<double> BackAPrice;
        public List<double> LayAPrice;
        public List<double> BackBPrice;
        public List<double> LayBPrice;
        public List<double> BackAAmount;
        public List<double> LayAAmount;
        public List<double> BackBAmount;
        public List<double> LayBAmount;

        public Market()
        {
        }

        public Market(List<double> _BackAPrice, List<double> _LayAPrice, List<double> _BackBPrice, List<double> _LayBPrice, List<double> _BackAAmount, List<double> _LayAAmount, List<double> _BackBAmount, List<double> _LayBAmount)
        {
            this.BackAPrice = _BackAPrice.ToList();               
            this.LayAPrice = _LayAPrice.ToList();
            this.BackBPrice = _BackBPrice.ToList();
            this.LayBPrice = _LayBPrice.ToList();
            this.BackAAmount = _BackAAmount.ToList();
            this.LayAAmount = _LayAAmount.ToList();
            this.BackBAmount = _BackBAmount.ToList();
            this.LayBAmount = _LayBAmount.ToList();
        }

        public TennisPrices.VjekosMarket CopyToVM()
        {
            TennisPrices.VjekosMarket Result = new TennisPrices.VjekosMarket();
            Result.BackAPrice = this.BackAPrice.ToList();
            Result.BackAAmount = this.BackAAmount.ToList();           
            Result.BackBAmount = this.BackBAmount.ToList();
            Result.BackBPrice = this.BackBPrice.ToList();
            Result.LayAAmount = this.LayAAmount.ToList();
            Result.LayAPrice = this.LayAPrice.ToList();
            Result.LayBAmount = this.LayBAmount.ToList();
            Result.LayBPrice = this.LayBPrice.ToList();

            return Result;
        }

        public Market CopyToMarket(TennisPrices.VjekosMarket MarketToCopy)
        {
            this.BackAAmount = MarketToCopy.BackAAmount.ToList();
            this.BackAPrice = MarketToCopy.BackAPrice.ToList();
            this.BackBAmount = MarketToCopy.BackBAmount.ToList();
            this.BackBPrice = MarketToCopy.BackBPrice.ToList();
            this.LayAAmount = MarketToCopy.LayAAmount.ToList();
            this.LayAPrice = MarketToCopy.LayAPrice.ToList();
            this.LayBAmount = MarketToCopy.LayBAmount.ToList();
            this.LayBPrice = MarketToCopy.LayBPrice.ToList();

            return this;
        }
    }
}
