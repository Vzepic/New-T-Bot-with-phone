using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace New_T_Bot
{
    #region Multiple Markets
    
    public class MultipleMarkets
    {
        public String MarketName;
        public int MarketId;
    }
    #endregion

    #region Class MarketRunners
    public class MarketRunners
    {
        public String RunnerName;
        public int SelectionId;
        public MarketRunners(String _RunnerName, int _SelectionId)
        {
            RunnerName = _RunnerName;
            SelectionId = _SelectionId;
        }
    }
    #endregion

    #region Class Bet

    public class Bet
    {
        public int selectionId;
        public double amount;
        public double price;
        public char backOrLay;

        public Bet()
        {
        }

        public Bet(int _selectionId, double _amount, double _price, char _backOrLay)
        {
            selectionId = _selectionId;
            amount = _amount;
            price = _price;
            backOrLay = _backOrLay;
        }

    }

    #endregion

    #region Class Multiple Market Bet

    public class MultipleMarketBet
    {
        public int selectionId;
        public double amount;
        public double price;
        public char backOrLay;
        public String MarketName;

        public MultipleMarketBet()
        {
        }

        public MultipleMarketBet(int _selectionId, double _amount, double _price, char _backOrLay, String _MarketName)
        {
            selectionId = _selectionId;
            amount = _amount;
            price = _price;
            backOrLay = _backOrLay;
            MarketName = _MarketName;
        }

    }

    #endregion

    #region Class Bet Results

    public class BetResults
    {
        public double averagePriceMatched;
        public long betId;
        public String resultCode;
        public double sizeMatched;
        public bool success;

        public BetResults(double _avgPriceMatched, long _betId, String _resultCode, double _sizeMatched, bool _success)
        {
            averagePriceMatched = _avgPriceMatched;
            betId = _betId;
            resultCode = _resultCode;
            sizeMatched = _sizeMatched;
            success = _success;
        }

        public BetResults()
        {
        }
    }

    #endregion

    #region Class Multiple Market Bet Results

    public class MultipleMarketBetResults
    {
        public double averagePriceMatched;
        public long betId;
        public String resultCode;
        public double sizeMatched;
        public bool success;
        public String MarketName;

        public MultipleMarketBetResults(double _avgPriceMatched, long _betId, String _resultCode, double _sizeMatched, bool _success, String _MarketName)
        {
            averagePriceMatched = _avgPriceMatched;
            betId = _betId;
            resultCode = _resultCode;
            sizeMatched = _sizeMatched;
            success = _success;
            MarketName = _MarketName;
        }

        public MultipleMarketBetResults()
        {
        }
    }
    #endregion

    #region Class CurrentMatchedPL

    public class CurrentPL
    {
        public double amount;
        public int selectionId;
    }

    #endregion

    #region Class Bet Status

    public class BetMUStatus
    {
        public double averagePriceMatched;
        public double sizeMatched;
        public double sizeUnmatched;
        public double priceRequested;
        public long betId;
        public int marketId;
        public int selectionId;
        public char betType;


        public BetMUStatus()
        {
        }
    }

    #endregion

    #region MarketDataType

    public class MarketDataType
    {
        public Int32 MarketID;
        public String MarketName;
        public String MarketType;
        public String MarketStatus;
        public DateTime EventDate;
        public String MenuPath;
        public String EventHeirachy;
        public String BetDelay;
        public Int32 ExchangeID;
        public String CountryCode;
        public DateTime LastRefresh;
        public Int32 NoOfRunners;
        public Int32 NoOfWinners;
        public Double TotalAmountMatched;
        public Boolean BSPMarket;
        public Boolean TurningInPlay;

        public String RelevantToString()
        {
            return MarketID.ToString() + " " + MarketStatus + " " + MarketName + " " + MenuPath;
        }
    } 

    #endregion

    #region Unpack Market Removed Runners

    public class UnpackMarketRemovedRunners
    {
        public String SelectionName;
        public DateTime RemovedDate;
        public String AdjusmentFactor;
    }

    #endregion

    #region Unpack Market Runner Information

    public class UnpackMarketRunnerInformation
    {
        public int SelectionId;
        public int OrderIndex;
        public double TotalAmountMatched;
        public double LastPriceMatched;
        public double Handicap = 0;
        public double ReductionFactor = 0;
        public bool Vacant = false;
        public double FairSpPrice = 0;
        public double NearSpPrice = 0;
        public double ActualSpPrice = 0;
        public List<UnpackMarketPrice> BackPrices = new List<UnpackMarketPrice>();
        public List<UnpackMarketPrice> LayPrices = new List<UnpackMarketPrice>();

        public void insertPrice(UnpackMarketPrice price)
        {
            if (price.BackOrLay == 'L' || price.BackOrLay == 'l')
                LayPrices.Add(price);
            else
                BackPrices.Add(price);
        }

        public void sortPrice()
        {
            LayPrices.Sort(priceCompareL);
            BackPrices.Sort(priceCompareB);
        }

        private int priceCompareL(UnpackMarketPrice price1, UnpackMarketPrice price2)
        {
            if (price1.Price == price2.Price)
                return 0;
            if (price1.Price > price2.Price)
                return 1;
            else
                return -1;
        }
        private int priceCompareB(UnpackMarketPrice price1, UnpackMarketPrice price2)
        {
            if (price1.Price == price2.Price)
                return 0;
            if (price1.Price > price2.Price)
                return -1;
            else
                return 1;
        }
    }

    #endregion

    #region Unpack Market Price

    public class UnpackMarketPrice
    {
        public double Price;
        public double AvaiableAmmount;
        public char BackOrLay;
        public int Depth;
    }

    #endregion

    #region MarketPrices

    public class MarketPrices
    {
        public int MarketId;
        public String CurencyCode;
        public String MarketStatus;
        public int InPlayDelay;
        public int NoOfWinners;
        public String MarketInformation;
        public bool DiscountAllowed;
        public String MarketBaseRate;
        public Int64 RefreshTime;
        public List<UnpackMarketRemovedRunners> RemovedRunners = new List<UnpackMarketRemovedRunners>();
        public bool BSPMarket;
        public List<UnpackMarketRunnerInformation> RunnerInformation = new List<UnpackMarketRunnerInformation>();

        private int begin = 0;
        private int end = 0;
        private String ToParse;

        private String GetNextField()
        {
            if (end != 0)
                begin = end + 1;
            end = ToParse.IndexOf('~', begin);
            return ToParse.Substring(begin, end - begin);
        }

        private void ParseRemovedRunners(String strRemovedRunners)
        {
            List<String> TempList = strRemovedRunners.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (int i = 0; i < TempList.Count; )
            {
                UnpackMarketRemovedRunners TempRunner = new UnpackMarketRemovedRunners();
                TempRunner.SelectionName = TempList[i];
                try
                {
                    TempRunner.RemovedDate = Convert.ToDateTime(TempList[i + 1]);
                }
                catch
                {
                    new DateTime(1970, 1, 1);
                }
                TempRunner.AdjusmentFactor = TempList[i + 2];
                i += 3;
                RemovedRunners.Add(TempRunner);
            }
        }

        public MarketPrices()
        {
        }
        public MarketPrices(String InMarketPrices)
        {
            ToParse = InMarketPrices;

            MarketId = Convert.ToInt32(GetNextField());
            CurencyCode = GetNextField();
            MarketStatus = GetNextField();
            InPlayDelay = Convert.ToInt32(GetNextField());
            NoOfWinners = Convert.ToInt32(GetNextField());
            MarketInformation = GetNextField();
            DiscountAllowed = Convert.ToBoolean(GetNextField());
            MarketBaseRate = GetNextField();
            RefreshTime = Convert.ToInt64(GetNextField());

            String strRemovedRunners = GetNextField();
            if (strRemovedRunners != "")
                ParseRemovedRunners(strRemovedRunners);

            if (ToParse[begin + 1] == 'N' || ToParse[begin + 1] == 'n')
                BSPMarket = false;
            else
                BSPMarket = true;
            begin += 2;

            List<String> TempRunners = ToParse.Substring(begin).Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (Char.IsDigit(TempRunners[0][0]) == false)
                TempRunners.RemoveAt(0);

            foreach (String runner in TempRunners)
            {
                UnpackMarketRunnerInformation tempRunner = new UnpackMarketRunnerInformation();
                ToParse = runner;
                begin = end = 0;

                tempRunner.SelectionId = Convert.ToInt32(GetNextField());
                tempRunner.OrderIndex = Convert.ToInt32(GetNextField());
                tempRunner.TotalAmountMatched = Convert.ToDouble(GetNextField());
                try
                {
                    tempRunner.LastPriceMatched = Convert.ToDouble(GetNextField());
                }
                catch
                {
                    tempRunner.LastPriceMatched = 0;
                }

                String RetValTemp;
                if ((RetValTemp = GetNextField()) != "")
                    tempRunner.Handicap = Convert.ToDouble(RetValTemp);
                if ((RetValTemp = GetNextField()) != "")
                    tempRunner.ReductionFactor = Convert.ToDouble(RetValTemp);
                if ((RetValTemp = GetNextField()) != "")
                    tempRunner.Vacant = Convert.ToBoolean(RetValTemp);
                if ((RetValTemp = GetNextField()) != "")
                    tempRunner.FairSpPrice = Convert.ToDouble(RetValTemp);
                if ((RetValTemp = GetNextField()) != "")
                    tempRunner.NearSpPrice = Convert.ToDouble(RetValTemp);
                if ((RetValTemp = GetNextField()) != "")
                    tempRunner.ActualSpPrice = Convert.ToDouble(RetValTemp);

                List<String> TempPrices = ToParse.Substring(begin + 1).Split(new char[] { '|', '~' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                bool start0 = true;

                if(TempPrices.Count > 0)
                {
                    try
                    {
                        //CRO
                        //Convert.ToDouble(TempPrices[0].Replace('.',','));
                        //UK
                        Convert.ToDouble(TempPrices[0]);
                        
                    }
                    catch
                    {
                        start0 = false;
                    }
                }

                for (int i = start0?0:1; i < TempPrices.Count; )
                {
                    //CRO
                    /*UnpackMarketPrice temp = new UnpackMarketPrice();
                    temp.Price = Convert.ToDouble(TempPrices[i].Replace('.', ','));
                    temp.AvaiableAmmount = Convert.ToDouble(TempPrices[i + 1].Replace('.', ','));
                    if (TempPrices[i + 2][0] == 'L' || TempPrices[i + 2][0] == 'l')
                        temp.BackOrLay = 'B';
                    else
                        temp.BackOrLay = 'L';
                    temp.Depth = Convert.ToInt32(TempPrices[i + 3]);
                    i += 4;
                    tempRunner.insertPrice(temp);*/
                    
                    //UK
                    UnpackMarketPrice temp = new UnpackMarketPrice();
                    temp.Price = Convert.ToDouble(TempPrices[i]);
                    temp.AvaiableAmmount = Convert.ToDouble(TempPrices[i + 1]);
                    if (TempPrices[i + 2][0] == 'L' || TempPrices[i + 2][0] == 'l')
                        temp.BackOrLay = 'B';
                    else
                        temp.BackOrLay = 'L';
                    temp.Depth = Convert.ToInt32(TempPrices[i + 3]);
                    i += 4;
                    tempRunner.insertPrice(temp);
                }
                tempRunner.sortPrice();
                RunnerInformation.Add(tempRunner);
            }
        }
    }
    #endregion

    #region Unpack All Markets

    public class UnpackAllMarkets
    {
        public List<MarketDataType> MarketData = new List<MarketDataType>();
        private DateTime BaseDate = new DateTime(1970, 1, 1);
        private String Mstring;

        public UnpackAllMarkets() { }

        public UnpackAllMarkets(String input)
        {
            if (input != "")
            {
                input = input.Replace("\\:", ".");
                Mstring = input.Replace(":", "~").Substring(1) + "~";

                do
                {
                    MarketDataType temp = new MarketDataType();
                    temp.MarketID = Convert.ToInt32(NextField());
                    temp.MarketName = NextField();
                    temp.MarketType = NextField();
                    temp.MarketStatus = NextField();
                    temp.EventDate = BaseDate.AddMilliseconds(Convert.ToDouble(NextField()));
                    temp.MenuPath = NextField();
                    temp.EventHeirachy = NextField();
                    temp.BetDelay = NextField();
                    temp.ExchangeID = Convert.ToInt32(NextField());
                    temp.CountryCode = NextField();
                    temp.LastRefresh = BaseDate.AddMilliseconds(Convert.ToDouble(NextField()));
                    temp.NoOfRunners = Convert.ToInt32(NextField());
                    temp.NoOfWinners = Convert.ToInt32(NextField());
                    temp.TotalAmountMatched = Convert.ToDouble(NextField());
                    temp.BSPMarket = (NextField() == "Y");
                    temp.TurningInPlay = (NextField() == "Y");
                    MarketData.Add(temp);
                }
                while (Mstring != "");
            }
        }

        private String NextField()
        {
            Int32 o;
            o = Mstring.IndexOf("~");
            String Next = Mstring.Substring(0, o);
            Mstring = Mstring.Substring(o + 1);
            return Next;
        }
    }

    #endregion

    #region Market Tree
    public class MarketInfoTreeContainer
    {
        public String MarketName;
        public Int32 MarketId;

        public MarketInfoTreeContainer()
        {
            MarketName = "";
            MarketId = 0;
        }
        public MarketInfoTreeContainer(String _MarketName, Int32 _MarketId)
        {
            MarketName = _MarketName;
            MarketId = _MarketId;
        }
    }

    public class MarketTree
    {
        public MarketInfoTreeContainer MarketInfo;
        List<MarketTree> Nodes;

        public MarketTree()
        {
            MarketInfo = new MarketInfoTreeContainer();
            Nodes = null;
        }

        public void AddNode(MarketInfoTreeContainer ChildrenMarketInfo)
        {
            if(Nodes == null)
                Nodes = new List<MarketTree>();
            Nodes.Add(new MarketTree { MarketInfo = ChildrenMarketInfo, Nodes = null });             
        }
    }
    #endregion

    #region Class Traded Volume
    public class RunnerPriceAndVolumeTraded
    {
        public double price;
        public double amount;
    };
    public class MarketPriceAndVolumeTraded
    {
        public Int32 SelectionId;
        public List<RunnerPriceAndVolumeTraded> PricesAndVolume;

        public MarketPriceAndVolumeTraded()
        {
            PricesAndVolume = new List<RunnerPriceAndVolumeTraded>();
        }
    };
    #endregion

    #region Class BetFair Ladder
    public static class BetFairLadder
    {
        private static double[] Ladder = new double[]
         #region odds
        {           
            1.01d,
            1.02d,
            1.03d,
            1.04d,
            1.05d,
            1.06d,
            1.07d,
            1.08d,
            1.09d,
            1.10d,
            1.11d,
            1.12d,
            1.13d,
            1.14d,
            1.15d,
            1.16d,
            1.17d,
            1.18d,
            1.19d,
            1.20d,
            1.21d,
            1.22d,
            1.23d,
            1.24d,
            1.25d,
            1.26d,
            1.27d,
            1.28d,
            1.29d,
            1.30d,
            1.31d,
            1.32d,
            1.33d,
            1.34d,
            1.35d,
            1.36d,
            1.37d,
            1.38d,
            1.39d,
            1.40d,
            1.41d,
            1.42d,
            1.43d,
            1.44d,
            1.45d,
            1.46d,
            1.47d,
            1.48d,
            1.49d,
            1.50d,
            1.51d,
            1.52d,
            1.53d,
            1.54d,
            1.55d,
            1.56d,
            1.57d,
            1.58d,
            1.59d,
            1.60d,
            1.61d,
            1.62d,
            1.63d,
            1.64d,
            1.65d,
            1.66d,
            1.67d,
            1.68d,
            1.69d,
            1.70d,
            1.71d,
            1.72d,
            1.73d,
            1.74d,
            1.75d,
            1.76d,
            1.77d,
            1.78d,
            1.79d,
            1.80d,
            1.81d,
            1.82d,
            1.83d,
            1.84d,
            1.85d,
            1.86d,
            1.87d,
            1.88d,
            1.89d,
            1.90d,
            1.91d,
            1.92d,
            1.93d,
            1.94d,
            1.95d,
            1.96d,
            1.97d,
            1.98d,
            1.99d,
            2.00d,
            2.02d,
            2.04d,
            2.06d,
            2.08d,
            2.10d,
            2.12d,
            2.14d,
            2.16d,
            2.18d,
            2.20d,
            2.22d,
            2.24d,
            2.26d,
            2.28d,
            2.30d,
            2.32d,
            2.34d,
            2.36d,
            2.38d,
            2.40d,
            2.42d,
            2.44d,
            2.46d,
            2.48d,
            2.50d,
            2.52d,
            2.54d,
            2.56d,
            2.58d,
            2.60d,
            2.62d,
            2.64d,
            2.66d,
            2.68d,
            2.70d,
            2.72d,
            2.74d,
            2.76d,
            2.78d,
            2.80d,
            2.82d,
            2.84d,
            2.86d,
            2.88d,
            2.90d,
            2.92d,
            2.94d,
            2.96d,
            2.98d,
            3.00d,
            3.05d,
            3.10d,
            3.15d,
            3.20d,
            3.25d,
            3.30d,
            3.35d,
            3.40d,
            3.45d,
            3.50d,
            3.55d,
            3.60d,
            3.65d,
            3.70d,
            3.75d,
            3.80d,
            3.85d,
            3.90d,
            3.95d,
            4.00d,
            4.1d,
            4.2d,
            4.3d,
            4.4d,
            4.5d,
            4.6d,
            4.7d,
            4.8d,
            4.9d,
            5.0d,
            5.1d,
            5.2d,
            5.3d,
            5.4d,
            5.5d,
            5.6d,
            5.7d,
            5.8d,
            5.9d,
            6.0d,
            6.2d,
            6.4d,
            6.6d,
            6.8d,
            7.0d,
            7.2d,
            7.4d,
            7.6d,
            7.8d,
            8.0d,
            8.2d,
            8.4d,
            8.6d,
            8.8d,
            9.0d,
            9.2d,
            9.4d,
            9.6d,
            9.8d,
            10.0d,
            10.5d,
            11.0d,
            11.5d,
            12.0d,
            12.5d,
            13.0d,
            13.5d,
            14.0d,
            14.5d,
            15.0d,
            15.5d,
            16.0d,
            16.5d,
            17.0d,
            17.5d,
            18.0d,
            18.5d,
            19.0d,
            19.5d,
            20d,
            21d,
            22d,
            23d,
            24d,
            25d,
            26d,
            27d,
            28d,
            29d,
            30d,
            32d,
            34d,
            36d,
            38d,
            40d,
            42d,
            44d,
            46d,
            48d,
            50d,
            55d,
            60d,
            65d,
            70d,
            75d,
            80d,
            85d,
            90d,
            95d,
            100d,
            110d,
            120d,
            130d,
            140d,
            150d,
            160d,
            170d,
            180d,
            190d,
            200d,
            210d,
            220d,
            230d,
            240d,
            250d,
            260d,
            270d,
            280d,
            290d,
            300d,
            310d,
            320d,
            330d,
            340d,
            350d,
            360d,
            370d,
            380d,
            390d,
            400d,
            410d,
            420d,
            430d,
            440d,
            450d,
            460d,
            470d,
            480d,
            490d,
            500d,
            510d,
            520d,
            530d,
            540d,
            550d,
            560d,
            570d,
            580d,
            590d,
            600d,
            610d,
            620d,
            630d,
            640d,
            650d,
            660d,
            670d,
            680d,
            690d,
            700d,
            710d,
            720d,
            730d,
            740d,
            750d,
            760d,
            770d,
            780d,
            790d,
            800d,
            810d,
            820d,
            830d,
            840d,
            850d,
            860d,
            870d,
            880d,
            890d,
            900d,
            910d,
            920d,
            930d,
            940d,
            950d,
            960d,
            970d,
            980d,
            990d,
            1000d
        };
#endregion

        public static double RoundPrice(double InPrice)
        {
            if (InPrice == Ladder[349])
                return Ladder[349];
            for (int i = 0; i < 349; i++)
            {
                if (InPrice == Ladder[i])
                    return Ladder[i];
                if (InPrice > Ladder[i] && InPrice < Ladder[i + 1])
                {
                    double diff1 = InPrice - Ladder[i];
                    double diff2 = Ladder[i + 1] - InPrice;

                    if (diff1 < diff2)
                        return Ladder[i];
                    else
                        return Ladder[i + 1];
                }
            }
            return 0;
        }

        public static double RoundPrice(double InPrice, char BackOrLay)
        {
            if (InPrice == Ladder[349])
                return Ladder[349];
            for (int i = 0; i < 349; i++)
            {
                if (InPrice == Ladder[i])
                    return Ladder[i];
                if (InPrice > Ladder[i] && InPrice < Ladder[i + 1])
                {
                    if (BackOrLay == 'B' || BackOrLay == 'b')
                        return Ladder[i + 1];
                    else
                        return Ladder[i];
                }
            }
            return 0;
        }

        public static double OnePriceHigher(double InPrice)
        {
            if (InPrice == Ladder[349])
                return Ladder[349];
            for (int i = 0; i < 349; i++)
            {
                if (InPrice >= Ladder[i] && InPrice < Ladder[i + 1])
                    return Ladder[i + 1];
            }
            return 0;
        }

        public static double OnePriceLower(double InPrice)
        {
            if (InPrice == Ladder[0])
                return Ladder[0];
            for (int i = 2; i < 350; i++)
            {
                if (InPrice > Ladder[i - 1] && InPrice <= Ladder[i])
                    return Ladder[i - 1];
            }
            return 0;
        }
    }
    #endregion
}