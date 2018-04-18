using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace New_T_Bot
{
    class Point
    {
        public Score CurrentScore;
        public string Type;
        public char PlayerWon;
        public Market BFMarket;

        public Point()
        {
        }

        public Point(Score _CurrentScore, string _Type, char _PlayerWon, Market _BFMarket)
        {
            this.CurrentScore = new Score
            {
                GamesA = _CurrentScore.GamesA,
                GamesB = _CurrentScore.GamesB,
                PlayerServing = _CurrentScore.PlayerServing,
                PointA = _CurrentScore.PointA,
                PointB = _CurrentScore.PointB,
                SetsA = _CurrentScore.SetsA,
                SetsB = _CurrentScore.SetsB,
                TiebreakNow = _CurrentScore.TiebreakNow
            };
            
            this.Type = _Type;
            this.PlayerWon = _PlayerWon;
            this.BFMarket = new Market
            {
                BackAAmount = _BFMarket.BackAAmount,
                BackAPrice = _BFMarket.BackAPrice,
                BackBAmount = _BFMarket.BackBAmount,
                BackBPrice = _BFMarket.BackBPrice,
                LayAAmount = _BFMarket.LayAAmount,
                LayAPrice = _BFMarket.LayAPrice,
                LayBAmount = _BFMarket.LayBAmount,
                LayBPrice = _BFMarket.LayBPrice
            };
        }

        public TennisPrices.VjekosPoint CopyToVP()
        {
            TennisPrices.VjekosPoint Result=new TennisPrices.VjekosPoint();
            Result.BFMarket = this.BFMarket.CopyToVM();
            Result.CurrentScore = this.CurrentScore.CopyToVS();
            Result.PlayerWon = this.PlayerWon;
            Result.Type = this.Type;

            return Result;
        }

        public Point CopyToPoint(TennisPrices.VjekosPoint PointToCopy)
        {
            this.BFMarket = this.BFMarket.CopyToMarket(PointToCopy.BFMarket);
            this.CurrentScore = this.CurrentScore.CopyToScore(PointToCopy.CurrentScore);
            this.PlayerWon = PointToCopy.PlayerWon;
            this.Type = PointToCopy.Type;

            return this;
        }
    }
}
