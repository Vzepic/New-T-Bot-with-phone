using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace TennisRatingPricesGet
{
    public class BetFairApi
    {
        #region Enumerators
        public enum Errors { OK = 1 , Invalide_Username_or_Password, Unexpected_Error, You_Have_To_login, Could_Not_Find_Sport, Could_Not_Find_Market, API_Error, Bet_Placement_Error, Bet_Canceling_Error, Get_Bet_Details_Erro }
        public enum Sports { Darts, Cricket, Tennis, Soccer, Snooker, Baseball, Rugby_League, Australian_Rules, Cross_Sport_Accumulators, Golf, Special_Bets, American_Football, Boxing, Ice_Hockey, Politics, Rugby_Union, Horse_Racing, Basketball };
        public enum BetStatus { Unmatched = 1, Matched };
        public enum UpdateBetReturns { OK = 1, Error, Bet_in_progress, Bet_taken_or_lapsed, Event_closed};
        #endregion

        #region Internal Declarations

        public String ErrorDetails;
        private BFGlobal.BFGlobalService BFGlobalService = new BFGlobal.BFGlobalService();
        private BFUK.BFExchangeService BetFairUK = new BFUK.BFExchangeService();
        private BFGlobal.APIRequestHeader APIRequestHeader = new BFGlobal.APIRequestHeader();

        public Errors Error;
        private System.Timers.Timer Timer;
        private int MarketId;
        private TimeSpan SearchInterval = new TimeSpan(10, 0, 0, 0);
        private TimeSpan PastSearchInterval = new TimeSpan(100, 0, 0, 0, 0);

        #endregion

        #region External Declarations

        private String RunnerA, RunnerB, MarketName;
        private Sports Sport;

        #endregion

        #region Constructor, Time Set

        public BetFairApi()
        {
        }

        public void TimeSet(TimeSpan _SearchInterval)
        {
            SearchInterval = _SearchInterval;
        }

        public void PastTimeSet(TimeSpan _PastTimeSpan)
        {
            PastSearchInterval = _PastTimeSpan;
        }

        #endregion

        #region Header Manipulation

        public void setSessionToken(BFGlobal.APIRequestHeader Header)
        {
            APIRequestHeader.sessionToken = Header.sessionToken;
        }

        public void setSessionToken(BFGlobal.APIResponseHeader Header)
        {
            APIRequestHeader.sessionToken = Header.sessionToken;
        }

        public void setSessionToken(BFUK.APIResponseHeader Header)
        {
            APIRequestHeader.sessionToken = Header.sessionToken;
        }

        public BFUK.APIRequestHeader GetUKHeader()
        {
            BFUK.APIRequestHeader Header = new BFUK.APIRequestHeader();
            Header.sessionToken = APIRequestHeader.sessionToken;
            return Header;
        }

        public BFGlobal.APIRequestHeader GetGlobalHeader()
        {
            return APIRequestHeader;
        }

        #endregion

        #region Login, Logout, Keep Alive, Timer

        private bool Login(String Username, String Password, int ApiId)
        {
            Error = Errors.OK;
            BFGlobal.LoginReq LoginReq = new BFGlobal.LoginReq();
            BFGlobal.LoginResp LoginResp = new BFGlobal.LoginResp();

            LoginReq.username = Username;
            LoginReq.password = Password;
            LoginReq.productId = ApiId;

            LoginResp = BFGlobalService.login(LoginReq);

            if (LoginResp.errorCode != BFGlobal.LoginErrorEnum.OK)
            {
                if (LoginResp.errorCode == BFGlobal.LoginErrorEnum.INVALID_USERNAME_OR_PASSWORD)
                    Error = Errors.Invalide_Username_or_Password;
                else
                {
                    Error = Errors.Unexpected_Error;
                    ErrorDetails = LoginResp.errorCode.ToString();
                }
                return false;
            }

            setSessionToken(LoginResp.header);

            Timer = new System.Timers.Timer(600000);
            Timer.Elapsed += KeepAlive;
            Timer.Start();

            return true;
        }

        public bool LoginFree(String Username, String Password)
        {
            return Login(Username, Password, 82);
        }

        public bool LoginNonFree(String Username, String Password)
        {
            return Login(Username, Password, 22);
        }

        public bool Logout()
        {
            Error = Errors.OK;
            BFGlobal.LogoutReq LogoutReq = new BFGlobal.LogoutReq();
            BFGlobal.LogoutResp LogoutResp = new BFGlobal.LogoutResp();

            LogoutReq.header = GetGlobalHeader();

            LogoutResp = BFGlobalService.logout(LogoutReq);

            if (LogoutResp.errorCode != BFGlobal.LogoutErrorEnum.OK)
            {
                Error = Errors.Unexpected_Error;
                ErrorDetails = LogoutResp.errorCode.ToString();
                return false;
            }

            return true;
        }

        private void KeepAlive(Object o, System.EventArgs a)
        {
            Error = Errors.OK;
            BFGlobal.KeepAliveReq KeepAliveReq = new BFGlobal.KeepAliveReq();
            BFGlobal.KeepAliveResp KeepAliveResp = new BFGlobal.KeepAliveResp();

            KeepAliveReq.header = GetGlobalHeader();

            try
            {
                KeepAliveResp = BFGlobalService.keepAlive(KeepAliveReq);
                setSessionToken(KeepAliveResp.header);
            }
            catch { }

            Timer.Start();
        }

        public void KeepAlive()
        {
            Error = Errors.OK;
            BFGlobal.KeepAliveReq KeepAliveReq = new BFGlobal.KeepAliveReq();
            BFGlobal.KeepAliveResp KeepAliveResp = new BFGlobal.KeepAliveResp();

            KeepAliveReq.header = GetGlobalHeader();

            KeepAliveResp = BFGlobalService.keepAlive(KeepAliveReq);

            setSessionToken(KeepAliveResp.header);
        }

        #endregion

        #region Get Market

        /// <summary>
        /// Manualy sets the market id inside this instance of api
        /// </summary>
        /// <param name="marketId">Desireg market id</param>

        public void ManualySetMarketId(int marketId)
        {
            MarketId = marketId;
        }

        public int GetMarketId()
        {
            try
            {
                return MarketId;
            }
            catch
            {
                return 0;
            }
        }

        public List<MarketPriceAndVolumeTraded> GetMarketTradedVolumeAndAmount()
        {
            Error = Errors.OK;
            BFUK.GetMarketTradedVolumeCompressedReq GetMarketTradedVolumeReq = new BFUK.GetMarketTradedVolumeCompressedReq();
            BFUK.GetMarketTradedVolumeCompressedResp GetMarketTradedVolumeResp = new BFUK.GetMarketTradedVolumeCompressedResp();

            GetMarketTradedVolumeReq.header = GetUKHeader();
            GetMarketTradedVolumeReq.marketId = MarketId;

            GetMarketTradedVolumeResp = BetFairUK.getMarketTradedVolumeCompressed(GetMarketTradedVolumeReq);

            if (GetMarketTradedVolumeResp.errorCode != BFUK.GetMarketTradedVolumeCompressedErrorEnum.OK)
            {
                Error = Errors.Unexpected_Error;
                ErrorDetails = GetMarketTradedVolumeResp.errorCode.ToString();
                return null;
            }
            List<String> FirstSplit = GetMarketTradedVolumeResp.tradedVolume.Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            List<MarketPriceAndVolumeTraded> retVal = new List<MarketPriceAndVolumeTraded>();

            foreach (String Runner in FirstSplit)
            {
                List<String> SecondSplit = Runner.Split(new String[] { "~", "|" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                MarketPriceAndVolumeTraded item = new MarketPriceAndVolumeTraded();
                item.SelectionId = Convert.ToInt32(SecondSplit[0]);

                for (int i = 5; i < SecondSplit.Count; i += 2)
                    item.PricesAndVolume.Add(new RunnerPriceAndVolumeTraded { price = Convert.ToDouble(SecondSplit[i]), amount = Convert.ToDouble(SecondSplit[i + 1]) });

                retVal.Add(item);
            }

            return retVal;
        }

        public String GetMarketName()
        {
            BFUK.GetMarketReq GetMarketReq = new BFUK.GetMarketReq();
            BFUK.GetMarketResp GetMarketResp = new BFUK.GetMarketResp();

            if(MarketId == 0)
                return null;

            GetMarketReq.header = GetUKHeader();
            GetMarketReq.marketId = MarketId;

            GetMarketResp = BetFairUK.getMarket(GetMarketReq);

            if (GetMarketResp.errorCode != BFUK.GetMarketErrorEnum.OK)
            {
                Error = Errors.API_Error;
                ErrorDetails = "Only 6 calls of this method per minute";
                return null;
            }

            return GetMarketResp.market.name;
        }

        public DateTime GetStartDate()
        {
            BFUK.GetMarketReq GetMarketReq = new BFUK.GetMarketReq();
            BFUK.GetMarketResp GetMarketResp = new BFUK.GetMarketResp();

            GetMarketReq.header = GetUKHeader();
            GetMarketReq.marketId = MarketId;

            GetMarketResp = BetFairUK.getMarket(GetMarketReq);

            if (GetMarketResp.errorCode != BFUK.GetMarketErrorEnum.OK)
            {
                Error = Errors.API_Error;
                ErrorDetails = "Only 6 calls of this method per minute";
                return new DateTime(0);
            }

            return GetMarketResp.market.marketTime;
        }

        private void TV_NodeMouseDoubleClick(object sender, System.Windows.Forms.TreeNodeMouseClickEventArgs e)
        {
            if (e.Clicks == 1)
                return;
            System.Windows.Forms.TreeNode TN = e.Node;

            if (TN.Nodes.Count != 0)
                return;

            MarketId = Convert.ToInt32(TN.Name);

            GUI.Close();            
        }
        private System.Windows.Forms.Form GUI;
        private UnpackAllMarkets SavedUnpackedMarkets = null;

        public bool GraphicMarketSelect(Sports _Sport, String Msg, bool Hold)
        {
            MarketId = 0;

            GUI = new System.Windows.Forms.Form();
            GUI.Height = 500;
            GUI.Width = 300;
            GUI.Name = Msg;
            GUI.Text = Msg;

            System.Windows.Forms.TreeView TV = new System.Windows.Forms.TreeView();
            TV.Height = 480;
            TV.Width = 280;
            TV.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(TV_NodeMouseDoubleClick);

            GUI.Controls.Add(TV);
            UnpackAllMarkets UnpackedMarkets;

            if ((Hold == false) || (Hold == true && SavedUnpackedMarkets == null))
            {
                Sport = _Sport;
                UnpackedMarkets = GetUnpackedMarkets(_Sport);

                if (Hold == true)
                    SavedUnpackedMarkets = UnpackedMarkets;
            }
            else
                UnpackedMarkets = SavedUnpackedMarkets;

            foreach (MarketDataType MarketData in UnpackedMarkets.MarketData)
            {
                String[] Temp = MarketData.EventHeirachy.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                List<String> IdHierarchy = new List<String>();
                foreach (String Line in Temp)
                    IdHierarchy.Add(Line);
                Temp = MarketData.MenuPath.Split(new String[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                List<String> PathHierarchy = new List<String>();

                bool toogle = true;
                foreach (String Line in Temp)
                {
                    if (Line.Contains("Group") && (IdHierarchy.Count != (Temp.Length + 1)))
                    {
                        if (toogle == true)
                            toogle = false;
                        else
                            PathHierarchy.Add(Line);
                    }
                    else
                        PathHierarchy.Add(Line);
                }
                PathHierarchy.Add(MarketData.MarketName);

                if (!TV.Nodes.ContainsKey(IdHierarchy[0]))
                    TV.Nodes.Add(IdHierarchy[0], PathHierarchy[0]);

                System.Windows.Forms.TreeNode TN = TV.Nodes[0];
                for (int i = 1; i < PathHierarchy.Count; i++)
                {
                    int Location = TN.Nodes.IndexOfKey(IdHierarchy[i]);
                    if (Location == -1)
                    {
                        TN.Nodes.Add(IdHierarchy[i], PathHierarchy[i]);
                        Location = TN.Nodes.IndexOfKey(IdHierarchy[i]);
                    }

                    TN = TN.Nodes[Location];
                }
            }

            GUI.ShowDialog();
            if (MarketId == 0)
                return false;
            return true;
        }


        /// <summary>
        /// Displays a new form with a tree like menu to select the desired market
        /// </summary>
        /// <param name="_Sport">Sports to look for</param>
        /// <returns>True if a market was selected</returns>
        /// 

        public bool GraphicMarketSelectWithDataHold(Sports _Sport)
        {
            return GraphicMarketSelect(_Sport, "Select Market", true);
        }

        public bool GraphicMarketSelect(Sports _Sport)
        {
            return GraphicMarketSelect(_Sport, "Select Market", false);
        }

        public void GraphicMarketSelectClearHoldData()
        {
            SavedUnpackedMarkets = null;
        }

        private UnpackAllMarkets GetUnpackedMarkets(Sports _Sport)
        {
            Sport = _Sport;

            Error = Errors.OK;

            if (GetGlobalHeader().sessionToken == "")
            {
                Error = Errors.You_Have_To_login;
                ErrorDetails = "Login first";
                return null;
            }

            int SportId = GetSportId();
            if (SportId == 0)
                return null;

            BFUK.GetAllMarketsReq GetAllMarketsReq = new BFUK.GetAllMarketsReq();
            BFUK.GetAllMarketsResp GetAllMarketsResp = new BFUK.GetAllMarketsResp();

            if (Sport == Sports.Cross_Sport_Accumulators)
            {
                int Sport1Id = GetSportId();
                Sport = Sports.Australian_Rules;
                int Sport2Id = GetSportId();
                Sport = Sports.Rugby_League;
                int Sport3Id = GetSportId();
                Sport = _Sport;
                GetAllMarketsReq.eventTypeIds = new int?[] { Sport1Id, Sport2Id, Sport3Id };
            }
            else
                GetAllMarketsReq.eventTypeIds = new int?[] { SportId };

            GetAllMarketsReq.header = GetUKHeader();
            GetAllMarketsReq.toDate = DateTime.Now.ToLocalTime().Add(SearchInterval);
            GetAllMarketsReq.fromDate = DateTime.Now.ToLocalTime().Subtract(PastSearchInterval);

            GetAllMarketsResp = BetFairUK.getAllMarkets(GetAllMarketsReq);

            setSessionToken(GetAllMarketsResp.header);

            if (GetAllMarketsResp.errorCode != BFUK.GetAllMarketsErrorEnum.OK)
            {
                Error = Errors.Unexpected_Error;
                ErrorDetails = GetAllMarketsResp.errorCode.ToString();
                return null;
            }

            UnpackAllMarkets UnpackedMarket = new UnpackAllMarkets(GetAllMarketsResp.marketData);
            return UnpackedMarket;
        }

        /// <summary>
        /// Searches betfair market for a match between RunnerA and a RunnerB and returns all markets associated with this event.
        /// </summary>
        /// <param name="_Sport">Sport in which to look</param>
        /// <param name="_RunnerA">Runner a</param>
        /// <param name="_RunnerB">Runner b</param>
        /// <returns>List of all avaiable markets</returns>
        /// 
        
        public String[] GetAvaiableMarkets(Sports _Sport, String _RunnerA, String _RunnerB)
        {
            Error = Errors.OK;
            Sport = _Sport;
            RunnerA = _RunnerA.ToUpper().Trim();
            RunnerB = _RunnerB.ToUpper().Trim();

            if (GetGlobalHeader().sessionToken == "")
            {
                Error = Errors.You_Have_To_login;
                ErrorDetails = "Login first";
                return new String[0];
            }

            int SportId = GetSportId();
            if(SportId == 0)
                return new String[0];

            BFUK.GetAllMarketsReq GetAllMarketsReq = new BFUK.GetAllMarketsReq();
            BFUK.GetAllMarketsResp GetAllMarketsResp = new BFUK.GetAllMarketsResp();

            GetAllMarketsReq.eventTypeIds = new  int?[] { SportId };
            GetAllMarketsReq.header = GetUKHeader();
            GetAllMarketsReq.fromDate = DateTime.Now.Subtract(PastSearchInterval);
            GetAllMarketsReq.toDate = DateTime.Now.Add(SearchInterval);

            GetAllMarketsResp = BetFairUK.getAllMarkets(GetAllMarketsReq);

            setSessionToken(GetAllMarketsResp.header);
            
            if (GetAllMarketsResp.errorCode != BFUK.GetAllMarketsErrorEnum.OK)
            {
                Error = Errors.Unexpected_Error;
                ErrorDetails = GetAllMarketsResp.errorCode.ToString();
                return new String[0];
            }

            UnpackAllMarkets UnpackedMarket = new UnpackAllMarkets(GetAllMarketsResp.marketData);

            List<String> Markets = new List<String>();

            foreach (MarketDataType Market in UnpackedMarket.MarketData)
                if (Market.MenuPath.ToUpper().Contains(RunnerA) && Market.MenuPath.ToUpper().Contains(RunnerB))
                    Markets.Add(Market.MarketName);

            return Markets.ToArray();
        }
        
        /// <summary>
        /// Tries to find the market
        /// </summary>
        /// <param name="_Sport">Sport to look for</param>
        /// <param name="_RunnerA">First runner to look for</param>
        /// <param name="_RunnerB">Second runner to look for</param>
        /// <param name="_MarketName">Name of the market to look for</param>
        /// <returns>True if the market was found</returns>
        public bool StartMarketSearch(Sports _Sport, String _RunnerA, String _RunnerB, String _MarketName)
        {
            Error = Errors.OK;
            Sport = _Sport;
            RunnerA = _RunnerA.ToUpper().Trim();
            RunnerB = _RunnerB.ToUpper().Trim();
            MarketName = _MarketName.ToUpper().Trim();

            if (GetGlobalHeader().sessionToken == "")
            {
                Error = Errors.You_Have_To_login;
                ErrorDetails = "Login first";
                return false;
            }
            
            int SportId = GetSportId();
            if (SportId == 0)
                return false;

            MarketId = GetMarketId(SportId);
            if (MarketId == 0)
                return false;

            return true;
        }

        private int GetSportId()
        {
            Error = Errors.OK;
            BFGlobal.GetEventTypesReq GetEventsReq = new BFGlobal.GetEventTypesReq();
            BFGlobal.GetEventTypesResp GetEventsResp = new BFGlobal.GetEventTypesResp();

            GetEventsReq.header = GetGlobalHeader();

            GetEventsResp = BFGlobalService.getActiveEventTypes(GetEventsReq);

            setSessionToken(GetEventsResp.header);

            if (GetEventsResp.errorCode != BFGlobal.GetEventsErrorEnum.OK)
            {
                Error = Errors.Unexpected_Error;
                return 0;
            }

            for (int i = 0; i < GetEventsResp.eventTypeItems.Length; i++)
                if (GetEventsResp.eventTypeItems[i].name == Sport.ToString().Replace('_', ' '))
                    return GetEventsResp.eventTypeItems[i].id;

            Error = Errors.Could_Not_Find_Sport;
            ErrorDetails = "Check Sport Spelling On Betfair.com";

            return 0;
        }

        /// <summary>
        /// Compressed
        /// </summary>
        /// <param name="sport"></param>
        /// <returns></returns>
        public UnpackAllMarkets GetMarkets(Sports sport)
        {
            return GetUnpackedMarkets(sport);    
        }

        private int GetMarketId(int SportId)
        {
            Error = Errors.OK;
            BFUK.GetAllMarketsReq GetAllMarketsReq = new BFUK.GetAllMarketsReq();
            BFUK.GetAllMarketsResp GetAllMarketsResp = new BFUK.GetAllMarketsResp();

            GetAllMarketsReq.eventTypeIds = new  int?[] { SportId };
            GetAllMarketsReq.header = GetUKHeader();
            GetAllMarketsReq.fromDate = DateTime.Now.Subtract(PastSearchInterval);
            GetAllMarketsReq.toDate = DateTime.Now.Add(SearchInterval);

            GetAllMarketsResp = BetFairUK.getAllMarkets(GetAllMarketsReq);

            setSessionToken(GetAllMarketsResp.header);

            if (GetAllMarketsResp.errorCode != BFUK.GetAllMarketsErrorEnum.OK)
            {
                Error = Errors.Unexpected_Error;
                ErrorDetails = GetAllMarketsResp.errorCode.ToString();
                return 0;
            }

            UnpackAllMarkets UnpackedMarket = new UnpackAllMarkets(GetAllMarketsResp.marketData);

            foreach (MarketDataType Market in UnpackedMarket.MarketData)
            {
                if (Market.MenuPath.ToUpper().Contains(RunnerA) && Market.MenuPath.ToUpper().Contains(RunnerB))
                    if (Market.MarketName.Contains(MarketName) || MarketName.Contains(Market.MarketName.ToUpper()))
                        return Market.MarketID;
            }

            Error = Errors.Could_Not_Find_Market;
            ErrorDetails = "Check Runner And Market Names On betfair.com";
            return 0;
        }

        /// <summary>
        /// Returns the array of Runners in the market previously selected
        /// </summary>
        /// <returns>Array of MarketRunners</returns>
        public MarketRunners[] GetMarketRunners()
        {
            Error = Errors.OK;
            BFUK.GetMarketReq GetMarketReq = new BFUK.GetMarketReq();
            BFUK.GetMarketResp GetMarketResp = new BFUK.GetMarketResp();

            GetMarketReq.header = GetUKHeader();
            GetMarketReq.marketId = MarketId;

            GetMarketResp = BetFairUK.getMarket(GetMarketReq);

            setSessionToken(GetMarketResp.header);

            List<MarketRunners> Runners = new List<MarketRunners>();

            if (GetMarketResp.errorCode != BFUK.GetMarketErrorEnum.OK)
            {
                Error = Errors.API_Error;
                ErrorDetails = "Only 6 calls of this method per minute";
                return Runners.ToArray();
            }

            for(int i = 0 ; i < GetMarketResp.market.runners.Length ; i++)
                Runners.Add(new MarketRunners(GetMarketResp.market.runners[i].name, GetMarketResp.market.runners[i].selectionId));

            return Runners.ToArray();                
        }

        /// <summary>
        /// Returns market prices on the preselected market. If there is no price offered it puts 1000 for the best lay price and 1.00 for best back price
        /// </summary>
        /// <returns>MarketPrices</returns>

        public MarketPrices GetMarketPrices2()
        {
            MarketPrices TempMP = GetMarketPrices();

            for (int i = 0; i < TempMP.RunnerInformation.Count; i++)
            {
                UnpackMarketRunnerInformation Runner = TempMP.RunnerInformation[i];
                UnpackMarketPrice Price;
                try
                {
                    Price = Runner.BackPrices[0];
                }
                catch 
                { 
                    Price = new UnpackMarketPrice();
                    Price.AvaiableAmmount = 2;
                    Price.BackOrLay = 'B';
                    Price.Depth = 1;
                    Price.Price = 1.00;
                    TempMP.RunnerInformation[i].BackPrices.Add(Price);
                }
                try
                {
                    Price = Runner.LayPrices[0];
                }
                catch
                {
                    Price = new UnpackMarketPrice();
                    Price.AvaiableAmmount = 2;
                    Price.BackOrLay = 'L';
                    Price.Depth = 1;
                    Price.Price = 1000;
                    TempMP.RunnerInformation[i].LayPrices.Add(Price);
                }
            }
            return TempMP;
        }

        /// <summary>
        /// Returns market prices but using a different method. Do not use with the free api 
        /// in transfer heavy applications. Do not use removed runners.
        /// </summary>
        /// <returns>Market prices</returns>
        public MarketPrices GetMarketPrices3()
        {
            Error = Errors.OK;
            BFUK.GetMarketPricesReq GetMarketPricesReq = new BFUK.GetMarketPricesReq();
            BFUK.GetMarketPricesResp GetMarketPricesResp = new BFUK.GetMarketPricesResp();

            GetMarketPricesReq.header = GetUKHeader();
            GetMarketPricesReq.marketId = MarketId;

            GetMarketPricesResp = BetFairUK.getMarketPrices(GetMarketPricesReq);

            if (GetMarketPricesResp.errorCode != BFUK.GetMarketPricesErrorEnum.OK) 
            {
                Error = Errors.API_Error;
                ErrorDetails = "Couldn't get market prices";
                return null;
            }

            MarketPrices Mp = new MarketPrices();

            Mp.BSPMarket = GetMarketPricesResp.marketPrices.bspMarket;
            Mp.CurencyCode = GetMarketPricesResp.marketPrices.currencyCode;
            Mp.DiscountAllowed = GetMarketPricesResp.marketPrices.discountAllowed;
            Mp.InPlayDelay = GetMarketPricesResp.marketPrices.delay;
            Mp.MarketBaseRate = GetMarketPricesResp.marketPrices.marketBaseRate.ToString();
            Mp.MarketId = GetMarketPricesResp.marketPrices.marketId;
            Mp.MarketInformation = GetMarketPricesResp.marketPrices.marketInfo;
            Mp.MarketStatus = GetMarketPricesResp.marketPrices.marketStatus.ToString();
            Mp.NoOfWinners = GetMarketPricesResp.marketPrices.numberOfWinners;
            Mp.RefreshTime = GetMarketPricesResp.marketPrices.lastRefresh;
            Mp.RunnerInformation = new List<UnpackMarketRunnerInformation>();

            foreach (BFUK.RunnerPrices Rp in GetMarketPricesResp.marketPrices.runnerPrices)
            {
                UnpackMarketRunnerInformation Umri = new UnpackMarketRunnerInformation();
                Umri.ActualSpPrice = Convert.ToDouble(Rp.actualBSP);
                Umri.FairSpPrice = Convert.ToDouble(Rp.farBSP);
                Umri.Handicap = Convert.ToDouble(Rp.handicap);
                Umri.LastPriceMatched = Convert.ToDouble(Rp.lastPriceMatched);
                Umri.NearSpPrice = Convert.ToDouble(Rp.nearBSP);
                Umri.OrderIndex = Rp.sortOrder;
                Umri.ReductionFactor = Rp.reductionFactor;
                Umri.SelectionId = Rp.selectionId;
                Umri.TotalAmountMatched = Rp.totalAmountMatched;
                Umri.Vacant = Convert.ToBoolean(Rp.vacant);
                Umri.BackPrices = new List<UnpackMarketPrice>();
                Umri.LayPrices = new List<UnpackMarketPrice>();

                int i = 0;
                foreach (BFUK.Price P in Rp.bestPricesToBack)
                {
                    if (i == 3)
                        break;
                    UnpackMarketPrice Ump = new UnpackMarketPrice();
                    Ump.AvaiableAmmount = P.amountAvailable;
                    Ump.Depth = P.depth;
                    Ump.Price = P.price;
                    Ump.BackOrLay = 'B';
                    Umri.BackPrices.Add(Ump);
                    i++;
                }
                i = 0;
                foreach (BFUK.Price P in Rp.bestPricesToLay)
                {
                    if (i == 3)
                        break;
                    UnpackMarketPrice Ump = new UnpackMarketPrice();
                    Ump.AvaiableAmmount = P.amountAvailable;
                    Ump.Depth = P.depth;
                    Ump.Price = P.price;
                    Ump.BackOrLay = 'L';
                    Umri.LayPrices.Add(Ump);
                    i++;
                }
                Mp.RunnerInformation.Add(Umri);
            }
            return Mp;
        }
        
        /// <summary>
        /// Returns complete market prices
        /// </summary>
        public MarketPrices GetMarketPrices4()
        {
            Error = Errors.OK;
            BFUK.GetCompleteMarketPricesCompressedReq GetMarketPricesCompressedReq = new BFUK.GetCompleteMarketPricesCompressedReq();
            BFUK.GetCompleteMarketPricesCompressedResp GetMarketPricesCompressedResp = new BFUK.GetCompleteMarketPricesCompressedResp();

            GetMarketPricesCompressedReq.header = GetUKHeader();
            GetMarketPricesCompressedReq.marketId = MarketId;

            try
            {
                GetMarketPricesCompressedResp = BetFairUK.getCompleteMarketPricesCompressed(GetMarketPricesCompressedReq);
            }
            catch
            {
                return null;
            }

            setSessionToken(GetMarketPricesCompressedResp.header);

            if (GetMarketPricesCompressedResp.errorCode != BFUK.GetCompleteMarketPricesErrorEnum.OK)
            {
                Error = Errors.API_Error;
                ErrorDetails = "Couldn't get market prices";
                return null;
            }

            MarketPrices UnpackedPrices = new MarketPrices();
            UnpackedPrices.RunnerInformation = new List<UnpackMarketRunnerInformation>();

            List<String> FirstSplit = GetMarketPricesCompressedResp.completeMarketPrices.Split(new char[]{':'}, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<String> SecondSplit = FirstSplit[0].Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<String> ThirdSplit;
            UnpackedPrices.MarketId = Convert.ToInt32(SecondSplit[0]);
            UnpackedPrices.InPlayDelay = Convert.ToInt32(SecondSplit[1]);

            for (int i = 1; i < FirstSplit.Count; i++)
            {
                UnpackMarketRunnerInformation RunnerInfo = new UnpackMarketRunnerInformation();
                RunnerInfo.BackPrices = new List<UnpackMarketPrice>();
                RunnerInfo.LayPrices = new List<UnpackMarketPrice>();
                SecondSplit = FirstSplit[i].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                ThirdSplit = SecondSplit[1].Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                SecondSplit = SecondSplit[0].Split(new char[] { '~' }).ToList();

                try
                {
                    RunnerInfo.SelectionId = Convert.ToInt32(SecondSplit[0]);
                }
                catch { RunnerInfo.SelectionId = 0; }
                try
                {
                    RunnerInfo.OrderIndex = Convert.ToInt32(SecondSplit[1]);
                }
                catch { RunnerInfo.OrderIndex = 0;  }
                try
                {
                    RunnerInfo.TotalAmountMatched = Convert.ToInt32(SecondSplit[2]);
                }
                catch { RunnerInfo.TotalAmountMatched = 0; }
                try
                {
                    RunnerInfo.LastPriceMatched = Convert.ToInt32(SecondSplit[3]);
                }
                catch { RunnerInfo.LastPriceMatched = 0;  }

                for (int j = 0; j < ThirdSplit.Count; j += 5)
                {
                    double odds = Convert.ToDouble(ThirdSplit[j]);
                    double backAvaiable = Convert.ToDouble(ThirdSplit[j + 1]);
                    double layAvaiable = Convert.ToDouble(ThirdSplit[j + 2]);

                    if(backAvaiable != 0)
                        RunnerInfo.BackPrices.Add(new UnpackMarketPrice
                        {
                            AvaiableAmmount = backAvaiable,
                            BackOrLay = 'B',
                            Price = odds
                        });
                    if(layAvaiable != 0)
                        RunnerInfo.LayPrices.Add(new UnpackMarketPrice
                        {
                            AvaiableAmmount = layAvaiable,
                            BackOrLay = 'L',
                            Price = odds
                        });
                }


                UnpackedPrices.RunnerInformation.Add(RunnerInfo);
            }

            return UnpackedPrices;
        }


        /// <summary>
        /// Returns market prices on the preselected market
        /// </summary>
        /// <returns>Market prices</returns>
        public MarketPrices GetMarketPrices()
        {
            Error = Errors.OK;
            BFUK.GetMarketPricesCompressedReq GetMarketPricesCompressedReq = new BFUK.GetMarketPricesCompressedReq();
            BFUK.GetMarketPricesCompressedResp GetMarketPricesCompressedResp = new BFUK.GetMarketPricesCompressedResp();

            GetMarketPricesCompressedReq.header = GetUKHeader();
            GetMarketPricesCompressedReq.marketId = MarketId;

            try
            {
                GetMarketPricesCompressedResp = BetFairUK.getMarketPricesCompressed(GetMarketPricesCompressedReq);
            }
            catch
            {
                return null;
            }

            setSessionToken(GetMarketPricesCompressedResp.header);

            if (GetMarketPricesCompressedResp.errorCode != BFUK.GetMarketPricesErrorEnum.OK)
            {
                Error = Errors.API_Error;
                ErrorDetails = "Couldn't get market prices";
                return null;
            }

            MarketPrices UnpackedPrices = new MarketPrices(GetMarketPricesCompressedResp.marketPrices);

            return UnpackedPrices;
        }

        /// <summary>
        /// Returns the current status of the market.
        /// </summary>
        /// <returns>True if market is in play</returns>
        public bool IsMarketInPlay()
        {
            BFUK.GetMarketInfoReq GetMarketInfoReq = new BFUK.GetMarketInfoReq();
            BFUK.GetMarketInfoResp GetMarketInfoResp = new BFUK.GetMarketInfoResp();

            Error = Errors.OK;
            ErrorDetails = "";

            GetMarketInfoReq.header = GetUKHeader();
            GetMarketInfoReq.marketId = MarketId;

            GetMarketInfoResp = BetFairUK.getMarketInfo(GetMarketInfoReq);

            setSessionToken(GetMarketInfoResp.header);

            if (GetMarketInfoResp.errorCode != BFUK.GetMarketErrorEnum.OK)
            {
                Error = Errors.Unexpected_Error;
                ErrorDetails = GetMarketInfoResp.errorCode.ToString();
                return false;
            }

            if (GetMarketInfoResp.marketLite.delay > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Returns the current status of the market
        /// </summary>
        /// <returns>True if market is active</returns>
        public bool IsMarketActive()
        {
            BFUK.GetMarketInfoReq GetMarketInfoReq = new BFUK.GetMarketInfoReq();
            BFUK.GetMarketInfoResp GetMarketInfoResp = new BFUK.GetMarketInfoResp();

            Error = Errors.OK;
            ErrorDetails = "";

            GetMarketInfoReq.header = GetUKHeader();
            GetMarketInfoReq.marketId = MarketId;

            GetMarketInfoResp = BetFairUK.getMarketInfo(GetMarketInfoReq);

            setSessionToken(GetMarketInfoResp.header);

            if (GetMarketInfoResp.errorCode != BFUK.GetMarketErrorEnum.OK)
            {
                Error = Errors.Unexpected_Error;
                ErrorDetails = GetMarketInfoResp.errorCode.ToString();
                return false;
            }

            if (GetMarketInfoResp.marketLite.marketStatus != BFUK.MarketStatusEnum.ACTIVE)
                return false;
            else
                return true;
        }

        #endregion

        #region Bets

        private double GetPrice(double price, char BackOrLay)
        {
            /*try
            {
                int decimalPlaces;
                double step;

                if (price <= 4)
                {
                    decimalPlaces = 2;
                    if (price <= 2)
                        step = 0.01d;
                    else if (price <= 3)
                        step = 0.02d;
                    else
                        step = 0.05d;
                }
                else if (price <= 20)
                {
                    decimalPlaces = 1;
                    if (price <= 6)
                        step = 0.1d;
                    else if (price <= 10)
                        step = 0.2d;
                    else
                        step = 0.5d;
                }
                else
                {
                    decimalPlaces = 0;
                    if (price <= 30)
                        step = 1d;
                    else if (price <= 50)
                        step = 2d;
                    else if (price <= 100)
                        step = 5d;
                    else
                        step = 10d;
                }

                double ProposedPrice;
                int powPlaces = Convert.ToInt32(Math.Pow(10, decimalPlaces));
                int MulPrice = Convert.ToInt32(price * powPlaces);
                int MulStep = Convert.ToInt32(Math.Round(step * powPlaces));
                int ModRem = MulPrice % MulStep;

                if (ModRem == 0)
                    ProposedPrice = Convert.ToDouble(MulPrice) / Convert.ToDouble(powPlaces);
                else
                {
                    if (BackOrLay == 'B' || BackOrLay == 'b')
                        ProposedPrice = Convert.ToDouble(MulPrice + MulStep - ModRem) / Convert.ToDouble(powPlaces);
                    else
                        ProposedPrice = Convert.ToDouble(MulPrice - ModRem) / Convert.ToDouble(powPlaces);
                }
                if (ProposedPrice < price && (BackOrLay == 'B' || BackOrLay == 'b'))
                    ProposedPrice += step;

                return ProposedPrice;
            }
            catch
            {
                return -1;
            }*/
            return BetFairLadder.RoundPrice(price, BackOrLay);
        }

        /// <summary>
        /// Rounds the given price to the one accepted by betfair. It is rouded to the better price depending if it's a back or lay price
        /// </summary>
        /// <param name="inPrice">Input price</param>
        /// <param name="BackOrLay">'B' for back 'L' for lay price</param>
        /// <returns>Rounded price</returns>

        public double RoundPrice(double inPrice, char BackOrLay)
        {            
            /*if (inPrice < 2)
                return Math.Round(inPrice, 2);
            else if (inPrice < 3)
            {
                double Round2 = Math.Round(inPrice, 2);
                Int32 Ostatak = Convert.ToInt32(Round2 * 100);
                if ((Ostatak % 2) == 0)
                    return Round2;
                else
                {
                    if (BackOrLay == 'B' || BackOrLay == 'b')
                        return Round2 + 0.01d;
                    else
                        return Round2 - 0.01d;
                }                
            }
            else if (inPrice < 4)
            {
                double Round2 = Math.Round(inPrice, 2);
                Int32 Ostatak = Convert.ToInt32(Round2 * 100) % 10;

                if (Ostatak == 5)
                    return Round2;
                else if (Ostatak < 5)
                {
                    if (BackOrLay == 'B' || BackOrLay == 'b')
                        return Round2 + (5 - Ostatak) * 0.01d;
                    else
                        return Round2 - Ostatak * 0.01d;
                }
                else
                {
                    if (BackOrLay == 'B' || BackOrLay == 'b')
                        return Round2 + (10 - Ostatak) * 0.01d;
                    else
                        return Round2 - (Ostatak - 5) * 0.01d;
                }
            }
            else if (inPrice < 6)
            {
                double Round2 = Math.Round(inPrice, 2);
                Int32 Ostatak = Convert.ToInt32(Round2 * 100) % 10;
                if (Ostatak == 0)
                    return Round2;
                else
                {
                    if (BackOrLay == 'B' || BackOrLay == 'b')
                        return Round2 + (10 - Ostatak) * 0.01d;
                    else
                        return Round2 - Ostatak * 0.01d;
                }
            }
            else if (inPrice < 10)
            {
                double Round2 = Math.Round(inPrice, 1);
                Int32 Ostatak = Convert.ToInt32(Round2 * 10) % 10;
                if (Ostatak % 2 == 0)
                    return Round2;
                else
                {
                    if (BackOrLay == 'B' || BackOrLay == 'b')
                        return Round2 + 0.1d;
                    else
                        return Round2 - 0.1d;
                }
            }
            else if (inPrice < 20)
            {
                double Round2 = Math.Round(inPrice, 1);
                Int32 Ostatak = Convert.ToInt32(Round2 * 10) % 10;
                if (Ostatak % 5 == 0)
                    return Round2;
                else if (Ostatak < 5)
                {
                    if (BackOrLay == 'B' || BackOrLay == 'b')
                        return Round2 + (5-Ostatak)*0.1d;
                    else
                        return Round2 - Ostatak * 0.1d;
                }
                else
                {
                    if (BackOrLay == 'B' || BackOrLay == 'b')
                        return Round2 + (10 - Ostatak) * 0.1d;
                    else
                        return Round2 - (Ostatak-5)* 0.1d;
                }
            }
            else if (inPrice < 50)
            {
                return Math.Round(inPrice, 0);
                
            }
            else if (inPrice < 100)
            {
                double Round2 = Math.Round(inPrice, 0);
                Int32 Ostatak = Convert.ToInt32(Round2) % 10;
                if (Ostatak % 5 == 0)
                    return Round2;
                else if (Ostatak < 5)
                {
                    if (BackOrLay == 'B' || BackOrLay == 'b')
                        return Round2 + (5 - Ostatak);
                    else
                        return Round2 - Ostatak;
                }
                else
                {
                    if (BackOrLay == 'B' || BackOrLay == 'b')
                        return Round2 + (10 - Ostatak);
                    else
                        return Round2 - (Ostatak - 5);
                }
            }
            else
            {
                double Round2 = Math.Round(inPrice, 0);
                Int32 Ostatak = Convert.ToInt32(Round2) % 10;
                if (Ostatak % 10 == 0)
                    return Round2;
                else
                {
                    if (BackOrLay == 'B' || BackOrLay == 'b')
                        return Round2 + (10-Ostatak);
                    else
                        return Round2 - Ostatak;
                }
            }*/
            return GetPrice(inPrice, BackOrLay);
        }

        /// <summary>
        /// Places a nonpersistant bet. All bets will be canceled when the market is turned into play
        /// </summary>
        /// <param name="bets">Array of bets</param>
        /// <returns>Array of BetResults</returns>
        public BetResults[] PlaceBet(Bet[] bets)
        {
            return PlaceBet(bets, 'N');
        }

        /// <summary>
        /// Places a persistant bet.
        /// </summary>
        /// <param name="bets">Array of bets</param>
        /// <returns>Array of BetResults</returns>
        public BetResults[] PlacePersistantBet(Bet[] bets)
        {
            return PlaceBet(bets, 'P');
        }

        private BetResults[] PlaceBet(Bet[] bets, char Persistancy)
        {

            
            Error = Errors.OK;
            BFUK.PlaceBetsReq PlaceBetsReq = new BFUK.PlaceBetsReq();
            BFUK.PlaceBetsResp PlaceBetsResp = new BFUK.PlaceBetsResp();

            PlaceBetsReq.header = GetUKHeader();

            List<BFUK.PlaceBets> BetList = new List<BFUK.PlaceBets>();

            for (int i = 0; i < bets.Length; i++)
            {
                if (bets[i].amount < 2)
                    continue;
                BFUK.PlaceBets TempBet = new BFUK.PlaceBets();

                TempBet.asianLineId = 0;
                TempBet.betCategoryType = BFUK.BetCategoryTypeEnum.E;

                if (Persistancy == 'N')
                    TempBet.betPersistenceType = BFUK.BetPersistenceTypeEnum.NONE;
                else
                    TempBet.betPersistenceType = BFUK.BetPersistenceTypeEnum.IP;

                if (bets[i].backOrLay == 'B' || bets[i].backOrLay == 'b')
                    TempBet.betType = BFUK.BetTypeEnum.B;
                else
                    TempBet.betType = BFUK.BetTypeEnum.L;
                TempBet.bspLiability = 0;
                TempBet.marketId = MarketId;
                TempBet.price = RoundPrice(bets[i].price, bets[i].backOrLay);
                TempBet.selectionId = bets[i].selectionId;
                TempBet.size = Math.Round(bets[i].amount, 2, MidpointRounding.ToEven);

                if(TempBet.price > 0)
                    BetList.Add(TempBet);
            }

            if (BetList.Count == 0)
            {
                Error = Errors.Bet_Placement_Error;
                return null;
            }

            PlaceBetsReq.bets = BetList.ToArray();
            PlaceBetsResp = BetFairUK.placeBets(PlaceBetsReq);

            if (PlaceBetsResp.errorCode != BFUK.PlaceBetsErrorEnum.OK)
            {
                Error = Errors.Bet_Placement_Error;
                ErrorDetails = PlaceBetsResp.errorCode.ToString();
                return null;
            }
            setSessionToken(PlaceBetsResp.header);

            List<BetResults> Results = new List<BetResults>();
            for (int i = 0; i < PlaceBetsResp.betResults.Length; i++)
                Results.Add(new BetResults(PlaceBetsResp.betResults[i].averagePriceMatched, PlaceBetsResp.betResults[i].betId, PlaceBetsResp.betResults[i].resultCode.ToString(), PlaceBetsResp.betResults[i].sizeMatched, PlaceBetsResp.betResults[i].success));

            return Results.ToArray();
        }

        public UpdateBetReturns UpdateBetAmount(long BetId, double Price, double oldAmount, double newAmount)
        {
            Error = Errors.OK;

            BFUK.UpdateBetsReq UpdateBetsReq = new BFUK.UpdateBetsReq();
            BFUK.UpdateBetsResp UpdateBetsResp = new BFUK.UpdateBetsResp();

            UpdateBetsReq.header = GetUKHeader();
            UpdateBetsReq.bets = new BFUK.UpdateBets[1];
            UpdateBetsReq.bets[0] = new BFUK.UpdateBets
                {
                    betId = BetId,
                    newPrice = Price,
                    newSize = newAmount,
                    oldPrice = Price,
                    oldSize = oldAmount,
                    newBetPersistenceType = BFUK.BetPersistenceTypeEnum.NONE,
                    oldBetPersistenceType = BFUK.BetPersistenceTypeEnum.NONE
                };

            UpdateBetsResp = BetFairUK.updateBets(UpdateBetsReq);
            setSessionToken(UpdateBetsResp.header);

            if (UpdateBetsResp.errorCode != BFUK.UpdateBetsErrorEnum.OK)
                return UpdateBetReturns.Error;
            if (UpdateBetsResp.betResults[0].success == true)
                return UpdateBetReturns.OK;
            if (UpdateBetsResp.betResults[0].resultCode == BFUK.UpdateBetsResultEnum.OK)
                return UpdateBetReturns.OK;
            if (UpdateBetsResp.betResults[0].resultCode == BFUK.UpdateBetsResultEnum.BET_IN_PROGRESS)
                return UpdateBetReturns.Bet_in_progress;
            if (UpdateBetsResp.betResults[0].resultCode == BFUK.UpdateBetsResultEnum.BET_TAKEN_OR_LAPSED)
                return UpdateBetReturns.Bet_taken_or_lapsed;
            if (UpdateBetsResp.betResults[0].resultCode == BFUK.UpdateBetsResultEnum.ERROR_LINE_EXPO_BET_CANCELLED_NOT_PLACED)
                return UpdateBetReturns.Bet_taken_or_lapsed;
            if (UpdateBetsResp.betResults[0].resultCode == BFUK.UpdateBetsResultEnum.EVENT_CLOSED_CANNOT_MODIFY_BET)
                return UpdateBetReturns.Event_closed;
            if (UpdateBetsResp.betResults[0].resultCode == BFUK.UpdateBetsResultEnum.NEW_BET_SUBMITTED_SUCCESSFULLY)
                return UpdateBetReturns.OK;
            if (UpdateBetsResp.betResults[0].resultCode == BFUK.UpdateBetsResultEnum.OK_REMAINING_CANCELLED)
                return UpdateBetReturns.OK;
            return
                UpdateBetReturns.Error;

        }

        /// <summary>
        /// Cancles all bets given in the BetResults array. Legacy name.
        /// </summary>
        /// <param name="BetResults">Array of bets to cancle</param>
        /// <returns>True if bets canceled</returns>

        public bool CancelAllBets(BetResults[] BetResults)
        {
            Error = Errors.OK;
            BFUK.CancelBetsReq CancelBetsReq = new BFUK.CancelBetsReq();
            BFUK.CancelBetsResp CancelBetsResp = new BFUK.CancelBetsResp();

            List<BFUK.CancelBets> BetsToCancel = new List<BFUK.CancelBets>();

            for (int i = 0; i < BetResults.Length; i++)
            {
                BFUK.CancelBets TempBet = new BFUK.CancelBets();
                TempBet.betId = BetResults[i].betId;
                BetsToCancel.Add(TempBet);
            }
            
            CancelBetsReq.header = GetUKHeader();
            CancelBetsReq.bets = BetsToCancel.ToArray();

            CancelBetsResp = BetFairUK.cancelBets(CancelBetsReq);

            setSessionToken(CancelBetsResp.header);

            if (CancelBetsResp.errorCode != BFUK.CancelBetsErrorEnum.OK)
            {
                Error = Errors.Bet_Canceling_Error;
                ErrorDetails = CancelBetsResp.errorCode.ToString();
                return false;
            }

            return true;
        }

        public void CancelBetsOnMultipleMarketsUsingBfFunction(Int32[] MarketsToCancel)
        {
            Error = Errors.OK;
            ErrorDetails = "";

            if (MarketsToCancel == null || MarketsToCancel.Length == 0)
                return;

            BFUK.CancelBetsByMarketReq CancelBetsByMarketReq = new BFUK.CancelBetsByMarketReq();
            BFUK.CancelBetsByMarketResp CancelBetsByMarketResp = new BFUK.CancelBetsByMarketResp();

            CancelBetsByMarketReq.header = GetUKHeader();
            CancelBetsByMarketReq.markets = new int?[MarketsToCancel.Length];
            for (int i = 0; i < MarketsToCancel.Length; i++)
                CancelBetsByMarketReq.markets[i] = (int?)MarketsToCancel[i];

            CancelBetsByMarketResp = BetFairUK.cancelBetsByMarket(CancelBetsByMarketReq);

            setSessionToken(CancelBetsByMarketResp.header);

            if (CancelBetsByMarketResp.errorCode != BFUK.CancelBetsByMarketErrorEnum.OK)
            {
                Error = Errors.Bet_Canceling_Error;
                ErrorDetails = CancelBetsByMarketResp.errorCode.ToString();
            }

        }


        /// <summary>
        /// Cancel all bets on the predefined market. This method is slow and if you have the BetResult array of all the bets use the other CancleAllBets function instead
        /// </summary>
        /// <returns></returns>
        /// 
        public bool CancelAllBets()
        {
            Error = Errors.OK;
            ErrorDetails = "";

            BetMUStatus[] BetStatus = GetCurrentUnmatchedBetStatus();

            if (Error != Errors.OK)
                return false;
            
            try
            {
                if (BetStatus.Length == 0)
                    return true;
            }
            catch
            {
                return true;
            }

            List<BetResults> ToCancle = new List<BetResults>();
            foreach (BetMUStatus Bet in BetStatus)
            {
                BetResults Temp = new BetResults();
                Temp.betId = Bet.betId;
                ToCancle.Add(Temp);
            }

            CancelAllBets(ToCancle.ToArray());

            if (Error != Errors.OK)
                return false;
            return true;
        }

        /// <summary>
        /// Get bet status for both matched and unmatched bets
        /// </summary>
        /// <returns>Array of BetMUStatus</returns>

        public BetMUStatus[] GetCurrentBetStatus()
        {
            return GetCurrentBetStatus('B');
        }

        /// <summary>
        /// Get the current unmatched bet status
        /// </summary>
        /// <returns>Array of BetMUStatus</returns>

        public BetMUStatus[] GetCurrentUnmatchedBetStatus()
        {
            return GetCurrentBetStatus('U');
        }

        public BetMUStatus[] GetCurrentMatchedBetStatus()
        {
            return GetCurrentBetStatus('M');
        }

        private BetMUStatus[] GetCurrentBetStatus(Char MatchedUnmatched)
        {
            Error = Errors.OK;
            BFUK.GetMUBetsReq GetMUBetsReq = new BFUK.GetMUBetsReq();
            BFUK.GetMUBetsResp GetMUBetsResp = new BFUK.GetMUBetsResp();

            GetMUBetsReq.header = GetUKHeader();
            if (MatchedUnmatched == 'B')
                GetMUBetsReq.betStatus = BFUK.BetStatusEnum.MU;
            else if (MatchedUnmatched == 'M')
                GetMUBetsReq.betStatus = BFUK.BetStatusEnum.M;
            else
                GetMUBetsReq.betStatus = BFUK.BetStatusEnum.U;
            GetMUBetsReq.marketId = MarketId;
            GetMUBetsReq.orderBy = BFUK.BetsOrderByEnum.BET_ID;
            GetMUBetsReq.recordCount = Int32.MaxValue - 1;
            GetMUBetsReq.sortOrder = BFUK.SortOrderEnum.ASC;
            GetMUBetsReq.startRecord = 0;

            GetMUBetsResp = BetFairUK.getMUBets(GetMUBetsReq);

            setSessionToken(GetMUBetsResp.header);

            if (GetMUBetsResp.errorCode != BFUK.GetMUBetsErrorEnum.OK)
            {
                Error = Errors.Get_Bet_Details_Erro;
                ErrorDetails = GetMUBetsResp.errorCode.ToString();
                return new BetMUStatus[0];
            }

            List<BetMUStatus> ReturnBets = new List<BetMUStatus>();

            foreach (BFUK.MUBet MuBet in GetMUBetsResp.bets)
            {
                BetMUStatus TempBet = new BetMUStatus();
                TempBet.betId = MuBet.betId;
                TempBet.marketId = MuBet.marketId;
                TempBet.selectionId = MuBet.selectionId;
                TempBet.betType = MuBet.betType.ToString()[0];
                if (MuBet.betStatus == BFUK.BetStatusEnum.M)
                {
                    TempBet.averagePriceMatched = MuBet.price;
                    TempBet.sizeMatched = MuBet.size;
                    TempBet.priceRequested = 0;
                    TempBet.sizeUnmatched = 0;
                }
                else if (MuBet.betStatus == BFUK.BetStatusEnum.U)
                {
                    TempBet.priceRequested = MuBet.price;
                    TempBet.sizeUnmatched = MuBet.size;
                    TempBet.averagePriceMatched = 0;
                    TempBet.sizeMatched = 0;
                }
                ReturnBets.Add(TempBet);
            }
            return ReturnBets.ToArray();
        }

        /// <summary>
        /// Get current profits and losses on the market. It includes unmatched bets.
        /// </summary>
        /// <returns>Array of CurrentPL</returns>

        public CurrentPL[] GetCurrentMatchedAndUnmatchedPL()
        {
            CurrentPL[] RetVal = GetCurrentMatchedPL();

            BetMUStatus[] UnmatchedBets = GetCurrentBetStatus('U');

            if (UnmatchedBets == null)
                return RetVal;

            for (int i = 0; i < RetVal.Length; i++)
            {
                for (int j = 0; j < UnmatchedBets.Length; j++)
                {
                    if (RetVal[i].selectionId == UnmatchedBets[j].selectionId)
                    {
                        if (UnmatchedBets[j].betType == 'B' || UnmatchedBets[j].betType == 'b')
                            RetVal[i].amount += (UnmatchedBets[j].priceRequested - 1) * UnmatchedBets[j].sizeUnmatched;
                        else
                            RetVal[i].amount -= (UnmatchedBets[j].priceRequested - 1) * UnmatchedBets[j].sizeUnmatched;
                    }
                    else
                    {
                        if (UnmatchedBets[j].betType == 'B' || UnmatchedBets[j].betType == 'b')
                            RetVal[i].amount -= UnmatchedBets[j].sizeUnmatched;
                        else
                            RetVal[i].amount += UnmatchedBets[j].sizeUnmatched; 
                    }
                }
            }
            return RetVal;
        }

        /// <summary>
        /// Get current profits and lossed only for matched bets without commision
        /// </summary>
        /// <returns>Array of CurrentPL</returns>
        public CurrentPL[] GetCurrentMatchedPL()
        {
            return GetCurrentMatchedPL(false);
        }

        /// <summary>
        /// Get current profit and loss for matched bets with or without commision
        /// </summary>
        /// <param name="Commission">True if commision should be calculated it the proffit and loss</param>
        /// <returns>Array of CurrentPL</returns>

        public CurrentPL[] GetCurrentMatchedPL(bool Commission)
        {
            Error = Errors.OK;
            ErrorDetails = "";
            
            BFUK.GetMarketProfitAndLossReq GetMarketProfitAndLossReq = new BFUK.GetMarketProfitAndLossReq();
            BFUK.GetMarketProfitAndLossResp GetMarketProfitAndLossResp = new BFUK.GetMarketProfitAndLossResp();

            GetMarketProfitAndLossReq.marketID = MarketId;
            GetMarketProfitAndLossReq.header = GetUKHeader();
            GetMarketProfitAndLossReq.netOfCommission = Commission;

            GetMarketProfitAndLossResp = BetFairUK.getMarketProfitAndLoss(GetMarketProfitAndLossReq);
            setSessionToken(GetMarketProfitAndLossResp.header);

            if (GetMarketProfitAndLossResp.errorCode != BFUK.GetMarketProfitAndLossErrorEnum.OK)
            {
                Error = Errors.Unexpected_Error;
                ErrorDetails = GetMarketProfitAndLossResp.errorCode.ToString();
                return null;
            }

            List<CurrentPL> retVal = new List<CurrentPL>();

            foreach (BFUK.ProfitAndLoss PL in GetMarketProfitAndLossResp.annotations)
            {
                CurrentPL Temp = new CurrentPL();
                Temp.amount = PL.ifWin;
                Temp.selectionId = PL.selectionId;
                retVal.Add(Temp);
            }

            return retVal.ToArray();
        }

        #endregion

        #region Wallet

        public double GetCurrentBalance()
        {
            BFUK.GetAccountFundsReq GetAccountFundsReq = new BFUK.GetAccountFundsReq();
            BFUK.GetAccountFundsResp GetAccountFundsResp = new BFUK.GetAccountFundsResp();

            GetAccountFundsReq.header = GetUKHeader();

            GetAccountFundsResp = BetFairUK.getAccountFunds(GetAccountFundsReq);

            Error = Errors.OK;
            ErrorDetails = "";

            if (GetAccountFundsResp.errorCode != BFUK.GetAccountFundsErrorEnum.OK)
            {
                Error = Errors.API_Error;
                ErrorDetails = GetAccountFundsResp.errorCode.ToString();
                return 0;
            }

            return Convert.ToDouble(GetAccountFundsResp.availBalance);
        }

        #endregion

        #region Helper Thingies

        public static double OnePriceLower(double Price)
        {
            return BetFairLadder.OnePriceLower(Price);
            /*
            double retVal;
            if (Price <= 2)
                retVal = Price - 0.01d;
            else if (Price <= 3)
                retVal = Price - 0.02d;
            else if (Price <= 4)
                retVal = Price - 0.05d;
            else if (Price <= 6)
                retVal = Price - 0.1d;
            else if (Price <= 10)
                retVal = Price - 0.2d;
            else if (Price <= 20)
                retVal = Price - 0.5d;
            else if (Price <= 30)
                retVal = Price - 1d;
            else if (Price <= 50)
                retVal = Price - 2d;
            else if (Price <= 100)
                retVal = Price - 5d;
            else
                retVal = Price - 10d;

            double Leftover = retVal * 100 -(Int32)(retVal * 100);
            double AbsLeftover = Math.Abs(Leftover);
            if (AbsLeftover < 0.1)
                retVal -= Leftover;
            return retVal;*/
        }

        public static double OnePriceHigher(double Price)
        {
            return BetFairLadder.OnePriceHigher(Price);
            /*if (Price < 2)
                return Price + 0.01d;
            else if (Price < 3)
                return Price + 0.02d;
            else if (Price < 4)
                return Price + 0.05d;
            else if (Price < 6)
                return Price + 0.1d;
            else if (Price < 10)
                return Price + 0.2d;
            else if (Price < 20)
                return Price + 0.5d;
            else if (Price < 30)
                return Price + 1d;
            else if (Price < 50)
                return Price + 2d;
            else if (Price < 100)
                return Price + 5d;
            else
                return Price + 10d;*/
        }

        public int GetDistanceInTicks(double price1, char price1backOrLay, double price2, char price2backOrLay)
        {
            bool price1notValid = false;
            bool price2notValid = false;

            if (price1 > 1000)
            {
                price1notValid = true;
                price1 = 1000;
            }
            if (price1 <= 1)
            {
                price1notValid = true;
                price1 = 1.01;
            }
            if (price2 > 1000)
            {
                price2notValid = true;
                price2 = 1000;
            }
            if (price2 <= 1)
            {
                price2notValid = true;
                price2 = 1.01;
            }

            double lPrice = RoundPrice(price1, price1backOrLay);
            double hPrice = RoundPrice(price2, price2backOrLay);

            if (lPrice == hPrice)
            {
                if (price1notValid == true || price2notValid == true)
                    return 1;
                return 0;
            }
            if (lPrice > hPrice)
            {
                double temp = lPrice;
                lPrice = hPrice;
                hPrice = temp;
            }

            int retVal = 0;

            while (lPrice < hPrice)
            {
                lPrice = OnePriceHigher(lPrice);
                retVal++;
            }
            
            if (price1notValid == true || price2notValid == true)
                return retVal + 1;
            return retVal;
        }

        #endregion
    }
}