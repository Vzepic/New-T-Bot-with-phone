using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace New_T_Bot
{   
        public class Score
        {
            public int PointA;
            public int PointB;
            public int GamesA;
            public int GamesB;
            public int SetsA;
            public int SetsB;
            public char PlayerServing;
            public bool TiebreakNow;

            public Score()
            {
            }

            public Score(int _PointA, int _PointB, int _GamesA, int _GamesB, int _SetsA, int _SetsB, char _PlayerServing, bool _TiebreakNow)
            {
                this.PointA = _PointA;
                this.PointB = _PointB;
                this.GamesA = _GamesA;
                this.GamesB = _GamesB;
                this.SetsA = _SetsA;
                this.SetsB = _SetsB;
                this.PlayerServing = _PlayerServing;
                this.TiebreakNow = _TiebreakNow;
            }

            public TennisPrices.VjekosScore CopyToVS()
            {
                TennisPrices.VjekosScore Result = new TennisPrices.VjekosScore();
                Result.PointA = this.PointA;
                Result.PointB = this.PointB;
                Result.GamesA = this.GamesA;
                Result.GamesB = this.GamesB;
                Result.SetsA = this.SetsA;
                Result.SetsB = this.SetsB;
                Result.PlayerServing = this.PlayerServing;
                Result.TiebreakNow = this.TiebreakNow;
                return Result;
            }

            public Score CopyToScore(TennisPrices.VjekosScore ScoreToCopy)
            {
                this.PointA = ScoreToCopy.PointA;
                this.PointB = ScoreToCopy.PointB;
                this.GamesA = ScoreToCopy.GamesA;
                this.GamesB = ScoreToCopy.GamesB;
                this.SetsA = ScoreToCopy.SetsA;
                this.SetsB = ScoreToCopy.SetsB;
                this.PlayerServing = ScoreToCopy.PlayerServing;
                this.TiebreakNow = ScoreToCopy.TiebreakNow;
                return this;
            }
        }  
}
