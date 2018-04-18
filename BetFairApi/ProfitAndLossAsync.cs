using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace TennisRatingPricesGet
{
    public delegate void Report();

    public class AsynBetStatus
    {
        public int selId;
        public double bAvgPrice;
        public double bAmount;
        public double lAvgPrice;
        public double lAmount;
    }

    public class AsynBetInfo
    {
        public int selId;
        public double bAvgPrice;
        public double bAmount;
        public double lAvgPrice;
        public double lAmount;
    }

    class ProfitAndLossChecker
    {
        String Username;
        String Password;
        int MarketId;
        int CheckTimeMs;
        BetFairApi Api;
        int noOr;
        AsynBetStatus[] retVal;
        
        private ProfitAndLossAsync PALC;

        public ProfitAndLossChecker(String username, String password, int marketId, int checkTimeMs, ProfitAndLossAsync Palc, int noOfRunners)
        {
            Username = username;
            Password = password;
            MarketId = marketId;
            CheckTimeMs = checkTimeMs;
            PALC = Palc;
            noOr = noOfRunners;
            retVal = new AsynBetStatus[noOr];
        }

        private AsynBetStatus[] GetCurrPlManual()
        {            
            BetMUStatus[] Bets = Api.GetCurrentMatchedBetStatus();
            
            //retVal = new AsynBetStatus[noOr];

            for (int i = 0; i < noOr; i++)
            {
                retVal[i].bAvgPrice = retVal[i].bAmount = retVal[i].lAmount = retVal[i].lAvgPrice = 0;
            }

            for(int i = 0 ; i < Bets.Count() ; i++)
                for(int j = 0 ; j < noOr ; j++)
                    if (Bets[i].selectionId == retVal[j].selId)
                    {
                        if (Bets[i].betType == 'B' || Bets[i].betType == 'b')
                        {
                            double win = (retVal[j].bAvgPrice - 1) * retVal[j].bAmount + (Bets[i].averagePriceMatched - 1) * Bets[i].sizeMatched;
                            retVal[j].bAmount = retVal[j].bAmount + Bets[i].sizeMatched;
                            retVal[j].bAvgPrice = 1 + win / retVal[j].bAmount;
                        }
                        else
                        {
                            double win = (retVal[j].lAvgPrice - 1) * retVal[j].lAmount + (Bets[i].averagePriceMatched - 1) * Bets[i].sizeMatched;
                            retVal[j].lAmount = retVal[j].lAmount + Bets[i].sizeMatched;
                            retVal[j].lAvgPrice = 1 + win / retVal[j].lAmount;
                        }
                        continue;
                    }
            return retVal;   
        }

        public void Start()
        {
            Api = new BetFairApi();
            Api.LoginNonFree(Username, Password);
            Api.ManualySetMarketId(MarketId);
            //Random rnd = new Random();
            MarketRunners[] runners = Api.GetMarketRunners();

            for(int i = 0 ; i < noOr ; i++)
            {
                retVal[i] = new AsynBetStatus();
                retVal[i].bAmount = 0;
                retVal[i].bAvgPrice = 0;
                retVal[i].lAmount = 0;
                retVal[i].lAvgPrice = 0;
                retVal[i].selId = runners[i].SelectionId;
            }

            while (true)
            {
                try
                {
                    //CurrentPL[] retVal = Api.GetCurrentMatchedPL();
                    AsynBetStatus[] retVal = GetCurrPlManual();

                    Report Rpt = new Report(delegate()
                    {
                        for (int i = 0; i < retVal.Count(); i++)
                        {
                            PALC.currBets[i].bAmount = retVal[i].bAmount;
                            PALC.currBets[i].bAvgPrice = retVal[i].bAvgPrice;
                            PALC.currBets[i].lAmount = retVal[i].lAmount;
                            PALC.currBets[i].lAvgPrice = retVal[i].lAvgPrice;
                            PALC.currBets[i].selId = retVal[i].selId;
                        }
                    });

                    Rpt.Invoke();
                }
                catch { }

                System.Threading.Thread.Sleep(CheckTimeMs);
            }
        }
    }
    
    class ProfitAndLossAsync
    {
        private String Username;
        private String Password;
        private int MarketId;
        private int CheckTimeMilliseconds;
        //public CurrentPL[] currPl;
        public AsynBetStatus[] currBets;
        private System.Threading.Thread t;
        private ProfitAndLossChecker PALS;
        private int noOr;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="username">Bf username</param>
        /// <param name="password">Bf password</param>
        /// <param name="marketId"> Market id</param>
        /// <param name="checkTimeMilliseconds">Time between checks of market</param>
        /// <param name="noOfRunners">Number of runners in the market</param>
        public ProfitAndLossAsync(String username, String password, int marketId, int checkTimeMilliseconds, int noOfRunners)
        {
            noOr = noOfRunners;
            Username = username;
            Password = password;
            MarketId = marketId;
            CheckTimeMilliseconds = checkTimeMilliseconds;
            currBets = new AsynBetStatus[noOfRunners];

            for (int i = 0; i < noOfRunners; i++)
            {
                currBets[i] = new AsynBetStatus();
                currBets[i].bAmount = currBets[i].bAvgPrice = currBets[i].lAmount = currBets[i].lAvgPrice = 0;
                currBets[i].selId = 0;
            }
        }

        /// <summary>
        /// Starts the async bet getting
        /// </summary>
        public void Start()
        {
            PALS = new ProfitAndLossChecker(Username, Password, MarketId, CheckTimeMilliseconds, this, noOr);
            t = new System.Threading.Thread(new System.Threading.ThreadStart(PALS.Start));
            t.Start();
        }

        /// <summary>
        /// Self explanatory
        /// </summary>
        public void Kill()
        {
            t.Abort();
        }

        /// <summary>
        /// Gets current average back and lay bets for every runner
        /// </summary>
        /// <returns></returns>
        public AsynBetStatus[] GetCurrBets()
        {
            AsynBetStatus[] retVal = new AsynBetStatus[currBets.Count()];

            for (int i = 0; i < currBets.Count(); i++)
            {
                retVal[i] = new AsynBetStatus();
                retVal[i].bAmount = currBets[i].bAmount;
                retVal[i].bAvgPrice = currBets[i].bAvgPrice;
                retVal[i].lAvgPrice = currBets[i].lAvgPrice;
                retVal[i].lAmount = currBets[i].lAmount;
                retVal[i].selId = currBets[i].selId;
            }
            return retVal;
        }

        /// <summary>
        /// Calculates last bets based on previous and newest information
        /// </summary>
        /// <param name="PreviousBets">Previous info</param>
        /// <returns>Probable last bets</returns>
        public AsynBetInfo[] CalculateLastBets(AsynBetStatus[] PreviousBets, AsynBetStatus[] NewBets)
        {
            AsynBetInfo[] RetVal = new AsynBetInfo[PreviousBets.Count()];

            for (int i = 0; i < PreviousBets.Count(); i++)
            {
                RetVal[i] = new AsynBetInfo();
                RetVal[i].selId = NewBets[i].selId;

                if ((PreviousBets[i].bAmount == NewBets[i].bAmount) && (PreviousBets[i].bAvgPrice == NewBets[i].bAvgPrice))
                    RetVal[i].bAmount = RetVal[i].bAvgPrice = 0;
                else
                {
                    double winDiff = (NewBets[i].bAvgPrice - 1) * NewBets[i].bAmount;
                    winDiff = winDiff - (PreviousBets[i].bAvgPrice - 1) * PreviousBets[i].bAmount;

                    RetVal[i].bAmount = NewBets[i].bAmount - PreviousBets[i].bAmount;
                    RetVal[i].bAvgPrice = winDiff / RetVal[i].bAmount + 1;
                }
                
                if ((PreviousBets[i].lAmount == NewBets[i].lAmount) && (PreviousBets[i].lAvgPrice == NewBets[i].lAvgPrice))
                    RetVal[i].lAmount = RetVal[i].lAvgPrice = 0;
                else
                {
                    double winDiff = (NewBets[i].lAvgPrice - 1) * NewBets[i].lAmount;
                    winDiff = winDiff - (PreviousBets[i].lAvgPrice - 1) * PreviousBets[i].lAmount;

                    RetVal[i].lAmount = NewBets[i].lAmount - PreviousBets[i].lAmount;
                    RetVal[i].lAvgPrice = winDiff / RetVal[i].lAmount + 1;
                }
            }
            return RetVal;
        }
    }
}
