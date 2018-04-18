using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace New_T_Bot
{
    class AsyncBetCancle
    {
        String username;
        String password;
        int MarketId;

        public AsyncBetCancle(String _user, String _pass,  int _MarketId)
        {
            username = _user;
            password = _pass;
            MarketId = _MarketId;
        }

        public void CancleBets()
        {
            try
            {
                BetFairApi API = new BetFairApi();
                API.LoginNonFree(username, password);
                API.ManualySetMarketId(MarketId);

                BetMUStatus[] bets = API.GetCurrentUnmatchedBetStatus();

                List<BetResults> BetsToCancle = new List<BetResults>();
                foreach (BetMUStatus kk in bets)
                {
                    BetResults temp = new BetResults(0, kk.betId, "", 0, true);
                    BetsToCancle.Add(temp);
                }


                API.CancelAllBets(BetsToCancle.ToArray());
                API.Logout();
            }
            catch { }
        }
    }
    
    class AsyncBetPlaceWithTimeCancle
    {
        MultipleMarketBet[] bets;
        int CancleTime;
        String username;
        String password;
        BFGlobal.APIRequestHeader header;

        public AsyncBetPlaceWithTimeCancle(MultipleMarketBet[] _bets, int _CancleTime, String _user, String _pass, BFGlobal.APIRequestHeader _header)
        {
            List<MultipleMarketBet> Temp = new List<MultipleMarketBet>();
            foreach (MultipleMarketBet bet in _bets)
                Temp.Add(bet);
            bets = Temp.ToArray();

            CancleTime = _CancleTime;
            username = _user;
            password = _pass;
            header = _header;
        }

        public void PlaceBets()
        {
            try
            {
                BetFairApi API = new BetFairApi();
                API.LoginNonFree(username, password);
                //API.setSessionToken(header);

                List<MultipleMarketBet> BetsToPlace;

                List<BetResults> AllBetsToCancle = new List<BetResults>();

                while (bets.Count() != 0)
                {
                    BetsToPlace = new List<MultipleMarketBet>();
                    BetsToPlace.Add(bets[0]);
                    List<int> IdxBetsToRemove = new List<int>();
                    IdxBetsToRemove.Add(0);
                    for (int i = 1; i < bets.Count(); i++)
                        if (bets[i].MarketName == BetsToPlace[0].MarketName)
                        {
                            BetsToPlace.Add(bets[i]);
                            IdxBetsToRemove.Add(i);
                        }

                    List<Bet> Bets = new List<Bet>();
                    for (int i = 0; i < BetsToPlace.Count; i++)
                    {
                        Bet TempBet = new Bet(BetsToPlace[i].selectionId, BetsToPlace[i].amount, BetsToPlace[i].price, BetsToPlace[i].backOrLay);
                        Bets.Add(TempBet);
                    }

                    API.ManualySetMarketId(Convert.ToInt32(BetsToPlace[0].MarketName));
                    BetResults[] BetsToCancle = API.PlaceBet(Bets.ToArray());

                    //dodat time delay
                    //API.CancelAllBets(BetsToCancle);
                    AllBetsToCancle.AddRange(BetsToCancle);

                    List<MultipleMarketBet> RemainingBets = new List<MultipleMarketBet>();

                    for (int i = 0; i < bets.Count(); i++)
                        if (!IdxBetsToRemove.Contains(i))
                            RemainingBets.Add(bets[i]);

                    bets = RemainingBets.ToArray();
                }
                System.Threading.Thread.Sleep(CancleTime);
                API.CancelAllBets(AllBetsToCancle.ToArray());
                //API.Logout();
            }
            catch { }
        }
    };

    class AsyncBetPlace
    {
        MultipleMarketBet[] bets;
        bool HitAndRun;
        String username;
        String password;
        BFGlobal.APIRequestHeader header;

        public AsyncBetPlace(MultipleMarketBet[] _bets, bool Hnr, String _user, String _pass, BFGlobal.APIRequestHeader _header)
        {
            List<MultipleMarketBet> Temp = new List<MultipleMarketBet>();
            foreach (MultipleMarketBet bet in _bets)
                Temp.Add(bet);
            bets = Temp.ToArray();

            HitAndRun = Hnr;
            username = _user;
            password = _pass;
            header = _header;
        }

        public void PlaceBets()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            try
            {
                BetFairApi API = new BetFairApi();
                //API.LoginNonFree(username, password);
                API.setSessionToken(header);

                List<MultipleMarketBet> BetsToPlace;

                while (bets.Count() != 0)
                {
                    BetsToPlace = new List<MultipleMarketBet>();
                    BetsToPlace.Add(bets[0]);
                    List<int> IdxBetsToRemove = new List<int>();
                    IdxBetsToRemove.Add(0);
                    for (int i = 1; i < bets.Count(); i++)
                        if (bets[i].MarketName == BetsToPlace[0].MarketName)
                        {
                            BetsToPlace.Add(bets[i]);
                            IdxBetsToRemove.Add(i);
                        }

                    List<Bet> Bets = new List<Bet>();
                    for (int i = 0; i < BetsToPlace.Count; i++)
                    {
                        Bet TempBet = new Bet(BetsToPlace[i].selectionId, BetsToPlace[i].amount, BetsToPlace[i].price, BetsToPlace[i].backOrLay);
                        Bets.Add(TempBet);
                    }

                    API.ManualySetMarketId(Convert.ToInt32(BetsToPlace[0].MarketName));
                    BetResults[] BetsToCancle = API.PlaceBet(Bets.ToArray());
                    if (API.Error != BetFairApi.Errors.OK)
                        Console.WriteLine(API.Error.ToString() + " " + API.ErrorDetails);
                    if (HitAndRun)
                        API.CancelAllBets(BetsToCancle);

                    List<MultipleMarketBet> RemainingBets = new List<MultipleMarketBet>();

                    for (int i = 0; i < bets.Count(); i++)
                        if (!IdxBetsToRemove.Contains(i))
                            RemainingBets.Add(bets[i]);

                    bets = RemainingBets.ToArray();
                }

                //API.Logout();
            }
            catch { }
            timer.Stop();
            Console.WriteLine(timer.ElapsedMilliseconds);
        }
    };
    
    public class MultipleMarketsBetFairApi
    {
        #region Enumerators
        public enum Errors { OK = 1 , Invalide_Username_or_Password, Unexpected_Error, You_Have_To_login, Could_Not_Find_Sport, Could_Not_Find_Market, API_Error, Bet_Placement_Error, Bet_Canceling_Error, Get_Bet_Details_Erro }
        public enum Sports { Darts, Cricket, Tennis, Soccer, Snooker, Baseball, Rugby_League, Australian_Rules, Cross_Sport_Accumulators, Golf, Special_Bets, American_Football, Boxing, Ice_Hockey, Politics, Rugby_Union, Horse_Racing, Basketball };
        public enum BetStatus { Unmatched = 1, Matched };
        #endregion

        #region Internal Declarations

        public String ErrorDetails;
        private BFGlobal.BFGlobalService BFGlobalService = new BFGlobal.BFGlobalService();
        private BFUK.BFExchangeService BetFairUK = new BFUK.BFExchangeService();
        private BFGlobal.APIRequestHeader APIRequestHeader = new BFGlobal.APIRequestHeader();
        public Errors Error;
        private System.Timers.Timer Timer;
        private int ParrentMarketId;
        private TimeSpan SearchInterval = new TimeSpan(10, 0, 0, 0);
        private TimeSpan PastSearchInterval = new TimeSpan(1, 0, 0);

        #endregion

        #region External Declarations

        private Sports Sport;

        #endregion

        #region Constructor, Time Set

        public MultipleMarketsBetFairApi()
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

        /// <summary>
        /// Login to the non free api
        /// </summary>
        /// <param name="Username">Username</param>
        /// <param name="Password">Password</param>
        /// <returns></returns>
        public bool LoginNonFree(String Username, String Password)
        {
            return Login(Username, Password, 22);
        }

        /// <summary>
        /// Login to the free api
        /// </summary>
        /// <param name="Username">Username</param>
        /// <param name="Password">Password</param>
        /// <returns></returns>
        public bool Login(String Username, String Password)
        {
            return Login(Username, Password, 82);
        }

        private bool Login(String Username, String Password, Int32 ProductId)
        {
            Error = Errors.OK;
            BFGlobal.LoginReq LoginReq = new BFGlobal.LoginReq();
            BFGlobal.LoginResp LoginResp = new BFGlobal.LoginResp();

            LoginReq.username = Username;
            LoginReq.password = Password;
            LoginReq.productId = ProductId;

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

        public List<MarketPriceAndVolumeTraded> GetMarketTradedVolumeAndAmount(String MarketName)
        {
            Error = Errors.OK;
            BFUK.GetMarketTradedVolumeCompressedReq GetMarketTradedVolumeReq = new BFUK.GetMarketTradedVolumeCompressedReq();
            BFUK.GetMarketTradedVolumeCompressedResp GetMarketTradedVolumeResp = new BFUK.GetMarketTradedVolumeCompressedResp();

            GetMarketTradedVolumeReq.header = GetUKHeader();
            GetMarketTradedVolumeReq.marketId = GetMarketId(MarketName);

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

                for(int i = 5 ; i < SecondSplit.Count ; i+=2)
                    item.PricesAndVolume.Add(new RunnerPriceAndVolumeTraded{price = Convert.ToDouble(SecondSplit[i]), amount = Convert.ToDouble(SecondSplit[i +1])});

                retVal.Add(item);
            }

            return retVal;
        }

        public double GetMarketTradedAmount(String MarketName)
        {
            Error = Errors.OK;
            BFUK.GetMarketTradedVolumeCompressedReq GetMarketTradedVolumeReq = new BFUK.GetMarketTradedVolumeCompressedReq();
            BFUK.GetMarketTradedVolumeCompressedResp GetMarketTradedVolumeResp = new BFUK.GetMarketTradedVolumeCompressedResp();

            GetMarketTradedVolumeReq.header = GetUKHeader();
            GetMarketTradedVolumeReq.marketId = GetMarketId(MarketName);

            GetMarketTradedVolumeResp = BetFairUK.getMarketTradedVolumeCompressed(GetMarketTradedVolumeReq);

            if (GetMarketTradedVolumeResp.errorCode != BFUK.GetMarketTradedVolumeCompressedErrorEnum.OK)
            {
                Error = Errors.Unexpected_Error;
                ErrorDetails = GetMarketTradedVolumeResp.errorCode.ToString();
                return 0;
            }
            double retVal = 0;
            List<String> FirstSplit = GetMarketTradedVolumeResp.tradedVolume.Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (String Runner in FirstSplit)
            {
                List<String> SecondSplit = Runner.Split(new String[] { "~", "|" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                for (int i = 6; i < SecondSplit.Count; i++)
                    retVal += Convert.ToDouble(SecondSplit[i]);
            }
            return retVal;
        }

        public UnpackAllMarkets GetUnpackedMarkets(Sports _Sport, TimeSpan SeekTime)
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

            GetAllMarketsReq.eventTypeIds = new int?[] { SportId };
            GetAllMarketsReq.toDate = DateTime.Now.ToLocalTime().Add(SearchInterval);
            GetAllMarketsReq.header = GetUKHeader();

            if (SeekTime != TimeSpan.Zero)
                GetAllMarketsReq.toDate = DateTime.Now.Add(SeekTime);

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

        public UnpackAllMarkets GetUnpackedMarkets(Sports _Sport)
        {
            return GetUnpackedMarkets(_Sport, TimeSpan.Zero);
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
                if (GetEventsResp.eventTypeItems[i].name == Sport.ToString())
                    return GetEventsResp.eventTypeItems[i].id;

            Error = Errors.Could_Not_Find_Sport;
            ErrorDetails = "Check Sport Spelling On Betfair.com";

            return 0;
        }

        public DateTime GetStartDate(String MarketName)
        {
            BFUK.GetMarketReq GetMarketReq = new BFUK.GetMarketReq();
            BFUK.GetMarketResp GetMarketResp = new BFUK.GetMarketResp();

            GetMarketReq.header = GetUKHeader();
            GetMarketReq.marketId = GetMarketByName(MarketName);

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

            if (TN.Nodes.Count == 0)
                return;

            ParrentMarketId = Convert.ToInt32(TN.Name);

            GUI.Close();            
        }
        private System.Windows.Forms.Form GUI;

        /// <summary>
        /// Displays a new form with a tree like menu to select the desired market
        /// </summary>
        /// <param name="_Sport">Sports to look for</param>
        /// <returns>True if a market was selected</returns>

        private UnpackAllMarkets UnpackedMarkets;
        public bool GraphicMarketSelect(Sports _Sport, bool showEndNodes)
        {
            Error = Errors.OK;
            ErrorDetails = "";
            
            ParrentMarketId = 0;

            GUI = new System.Windows.Forms.Form();
            GUI.Height = 800;
            GUI.Width = 300;
            GUI.Name = "Select Market";

            System.Windows.Forms.TreeView TV = new System.Windows.Forms.TreeView();
            TV.Height = 780;
            TV.Width = 280;
            TV.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(TV_NodeMouseDoubleClick);

            GUI.Controls.Add(TV);

            Sport = _Sport;

            UnpackedMarkets = GetUnpackedMarkets(_Sport);

            /*foreach (MarketDataType MarketData in UnpackedMarkets.MarketData)
            {
                String[] Temp = MarketData.EventHeirachy.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                List<String> IdHierarchy = new List<String>();
                foreach (String Line in Temp)
                    IdHierarchy.Add(Line);
                Temp = MarketData.MenuPath.Split(new String[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                List<String> PathHierarchy = new List<String>();
                foreach (String Line in Temp)
                    if (!Line.Contains("Group") || Sport==Sports.Darts)
                        PathHierarchy.Add(Line);
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
            }*/

            /*foreach (MarketDataType MarketData in UnpackedMarkets.MarketData)
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
                    if (Line.Contains("Group"))
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
            }*/

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
            if (ParrentMarketId == 0)
                return false;
            return true;
        }


        /// <summary>
        /// Returns all the markets associated with previously selected event
        /// </summary>
        /// <returns></returns>

        private List<MultipleMarkets> SelectedMultipleMarkets;

        public void ManualySetSelectedMultipleMarkets(List<MultipleMarkets> Markets)
        {
            SelectedMultipleMarkets = new List<MultipleMarkets>();
            foreach(MultipleMarkets Market in Markets)
            {
                SelectedMultipleMarkets.Add(new MultipleMarkets
                {
                    MarketId = Market.MarketId,
                    MarketName = Market.MarketName
                });
            }
        }

        public String[] GetMarkets()
        {
            SelectedMultipleMarkets = new List<MultipleMarkets>();
            if(ParrentMarketId == 0)
                return null;

            List<String> retVal = new List<String>();
            foreach(MarketDataType MarketData in UnpackedMarkets.MarketData)
            {
                if (MarketData.EventHeirachy.Contains(ParrentMarketId.ToString()))
                {
                    String Parser = MarketData.MenuPath;
                    int begin = Parser.LastIndexOf("\\") + 1;
                    Parser = Parser.Substring(begin);
                    
                    MultipleMarkets TempMultipleMarkets = new MultipleMarkets();
                    TempMultipleMarkets.MarketName = Parser + "\\" + MarketData.MarketName;
                    TempMultipleMarkets.MarketId = MarketData.MarketID;

                    SelectedMultipleMarkets.Add(TempMultipleMarkets);
                    retVal.Add(Parser + "\\" + MarketData.MarketName);
                }
            }
            return retVal.ToArray();
        }

        private int GetMarketByName(String MarketName)
        {
            foreach (MultipleMarkets MM in SelectedMultipleMarkets)
                if (MM.MarketName == MarketName)
                    return MM.MarketId;
            return 0;
        }

        /// <summary>
        /// Returns the array of Runners in the market previously selected
        /// </summary>
        /// <returns>Array of MarketRunners</returns>
        public MarketRunners[] GetMarketRunners(String MarketName)
        {
            Error = Errors.OK;
            BFUK.GetMarketReq GetMarketReq = new BFUK.GetMarketReq();
            BFUK.GetMarketResp GetMarketResp = new BFUK.GetMarketResp();

            GetMarketReq.header = GetUKHeader();
            GetMarketReq.marketId = GetMarketByName(MarketName);

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
        /// Returns market prices on the preselected market
        /// </summary>
        /// <returns>Market prices</returns>
        public MarketPrices GetMarketPrices(String MarketName)
        {
            Error = Errors.OK;
            BFUK.GetMarketPricesCompressedReq GetMarketPricesCompressedReq = new BFUK.GetMarketPricesCompressedReq();
            BFUK.GetMarketPricesCompressedResp GetMarketPricesCompressedResp = new BFUK.GetMarketPricesCompressedResp();

            GetMarketPricesCompressedReq.header = GetUKHeader();
            GetMarketPricesCompressedReq.marketId = GetMarketByName(MarketName);

            
            /////
            GetMarketPricesCompressedResp = BetFairUK.getMarketPricesCompressed(GetMarketPricesCompressedReq);

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
        public bool IsMarketInPlay(String MarketName)
        {
            BFUK.GetMarketInfoReq GetMarketInfoReq = new BFUK.GetMarketInfoReq();
            BFUK.GetMarketInfoResp GetMarketInfoResp = new BFUK.GetMarketInfoResp();

            Error = Errors.OK;
            ErrorDetails = "";

            GetMarketInfoReq.header = GetUKHeader();
            GetMarketInfoReq.marketId = GetMarketByName(MarketName);

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
        #endregion

        #region Bets

        private double GetPrice(double price, char BackOrLay)
        {
            if (price < 1)
                return -1;
            
            try
            {
                int decimalPlaces;
                double step;

                if (price <= 4)
                {
                    decimalPlaces = 2;
                    if (price <= 2)
                        step = 0.01;
                    else if (price <= 3)
                        step = 0.02;
                    else
                        step = 0.05;
                }
                else if (price <= 20)
                {
                    decimalPlaces = 1;
                    if (price <= 6)
                        step = 0.1;
                    else if (price <= 10)
                        step = 0.2;
                    else
                        step = 0.5;
                }
                else
                {
                    decimalPlaces = 0;
                    if (price <= 30)
                        step = 1;
                    else if (price <= 50)
                        step = 2;
                    else if (price <= 100)
                        step = 5;
                    else
                        step = 10;
                }

                double ProposedPrice;

                int powPlaces = Convert.ToInt32(Math.Pow(10, decimalPlaces));
                int MulPrice = Convert.ToInt32(price * powPlaces);
                int MulStep = Convert.ToInt32(Math.Round(step * powPlaces));
                int ModRem = MulPrice % MulStep;

                if (ModRem == 0)
                    ProposedPrice = (double)MulPrice / (double)powPlaces;
                else
                {
                    if (BackOrLay == 'B' || BackOrLay == 'b')
                        ProposedPrice = (double)(MulPrice + MulStep - ModRem) / (double)powPlaces;
                    else
                        ProposedPrice = (double)(MulPrice - ModRem) / (double)powPlaces;
                }
                if (ProposedPrice < price && (BackOrLay == 'B' || BackOrLay == 'b'))
                    ProposedPrice += step;

                return ProposedPrice;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Rounds the given price to the one accepted by betfair. It is rouded to the better price depending if it's a back or lay price
        /// </summary>
        /// <param name="inPrice">Input price</param>
        /// <param name="BackOrLay">'B' for back 'L' for lay price</param>
        /// <returns>Rounded price</returns>

        public double RoundPrice(double inPrice, char BackOrLay)
        {            
            /*
             * if (inPrice < 2)
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
            }
             * */
            return GetPrice(inPrice, BackOrLay);
        }

        /// <summary>
        /// Places a nonpersistant bet. All bets will be canceled when the market is turned into play
        /// </summary>
        /// <param name="bets">Array of bets</param>
        /// <returns>Array of BetResults</returns>
        public BetResults[] PlaceBet(MultipleMarketBet[] bets)
        {
            return PlaceBet(bets, 'N');
        }

        /// <summary>
        /// Places a persistant bet.
        /// </summary>
        /// <param name="bets">Array of bets</param>
        /// <returns>Array of BetResults</returns>
        public BetResults[] PlacePersistantBet(MultipleMarketBet[] bets)
        {
            return PlaceBet(bets, 'P');
        }

        /// <summary>
        /// Places bets on multiple markets.
        /// </summary>
        /// <param name="bets"></param>
        /// <returns></returns>
        public void PlaceBetsOnMultipleMarkets(MultipleMarketBet[] bets)
        {
            List<MultipleMarketBet> BetsToPlace;

            while (bets.Count() != 0)
            {
                BetsToPlace = new List<MultipleMarketBet>();
                BetsToPlace.Add(bets[0]);
                List<int> IdxBetsToRemove = new List<int>();
                IdxBetsToRemove.Add(0);
                for(int i = 1 ; i < bets.Count() ; i++)
                    if (bets[i].MarketName == BetsToPlace[0].MarketName)
                    {
                        BetsToPlace.Add(bets[i]);
                        IdxBetsToRemove.Add(i);
                    }

                PlaceBet(BetsToPlace.ToArray());
                List<MultipleMarketBet> RemainingBets = new List<MultipleMarketBet>();

                for (int i = 0; i < bets.Count(); i++)
                    if (!IdxBetsToRemove.Contains(i))
                        RemainingBets.Add(bets[i]);

                bets = RemainingBets.ToArray();
            }
        }

        public void PlaceAsyncBetsOnMultipleMarkets(MultipleMarketBet[] bets, bool HitAndRun, string username, string password)
        {
            List<MultipleMarketBet> BetsToPlace;

            while (bets.Count() != 0)
            {
                BetsToPlace = new List<MultipleMarketBet>();
                BetsToPlace.Add(bets[0]);
                List<int> IdxBetsToRemove = new List<int>();
                IdxBetsToRemove.Add(0);
                for (int i = 1; i < bets.Count(); i++)
                    if (bets[i].MarketName == BetsToPlace[0].MarketName)
                    {
                        BetsToPlace.Add(bets[i]);
                        IdxBetsToRemove.Add(i);
                    }

                PlaceAsyncBets(BetsToPlace.ToArray(), HitAndRun, username, password);
                List<MultipleMarketBet> RemainingBets = new List<MultipleMarketBet>();

                for (int i = 0; i < bets.Count(); i++)
                    if (!IdxBetsToRemove.Contains(i))
                        RemainingBets.Add(bets[i]);

                bets = RemainingBets.ToArray();
            }
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
        /// Cancle all bets on all markets
        /// </summary>
        public void CancleAllBetsOnMultipleMarkets()
        {
            foreach (MultipleMarkets Market in SelectedMultipleMarkets)
                CancelAllBets(Market.MarketName);
        }

        public int GetMarketId(String MarketName)
        {
            return GetMarketByName(MarketName);
        }

        public void PlaceAsyncBets(MultipleMarketBet[] _Bets, bool HitAndRun, string username, string password)
        {
            MultipleMarketBet[] Bets = new MultipleMarketBet[_Bets.Count()];
            for (int i = 0; i < _Bets.Count(); i++)
            {
                Bets[i] = new MultipleMarketBet();
                Bets[i].amount = _Bets[i].amount;
                Bets[i].backOrLay = _Bets[i].backOrLay;
                Bets[i].MarketName = _Bets[i].MarketName;
                Bets[i].price = _Bets[i].price;
                Bets[i].selectionId = _Bets[i].selectionId;
            }

           for (int i = 0; i < Bets.Count(); i++)
                Bets[i].MarketName = GetMarketByName(Bets[i].MarketName).ToString();

            AsyncBetPlace Abp = new AsyncBetPlace(Bets, HitAndRun, username, password, APIRequestHeader);

            new Thread(new ThreadStart(Abp.PlaceBets)).Start();
        }

        public void CancleMultipleMarketBetsAsync(String[] marketName, string username, string password)
        {
            foreach (String market in marketName)
                CancleBetsAsync(market, username, password);
        }
        
        public void CancleBetsAsync(String marketName, string username, string password)
        {
            AsyncBetCancle abc = new AsyncBetCancle(username, password, GetMarketByName(marketName));
            new Thread(new ThreadStart(abc.CancleBets)).Start();
        }

        public void PlaceAsyncBetsWithTimeCancle(MultipleMarketBet[] _Bets, int CancleTime, string username, string password)
        {
            MultipleMarketBet[] Bets = new MultipleMarketBet[_Bets.Count()];
            for (int i = 0; i < _Bets.Count(); i++)
            {
                Bets[i] = new MultipleMarketBet();
                Bets[i].amount = _Bets[i].amount;
                Bets[i].backOrLay = _Bets[i].backOrLay;
                Bets[i].MarketName = _Bets[i].MarketName;
                Bets[i].price = _Bets[i].price;
                Bets[i].selectionId = _Bets[i].selectionId;
            }

            for (int i = 0; i < Bets.Count(); i++)
                Bets[i].MarketName = GetMarketByName(Bets[i].MarketName).ToString();

            AsyncBetPlaceWithTimeCancle Abp = new AsyncBetPlaceWithTimeCancle(Bets, CancleTime, username, password, APIRequestHeader);

            new Thread(new ThreadStart(Abp.PlaceBets)).Start();
        }

        private BetResults[] PlaceBet(MultipleMarketBet[] bets, char Persistancy)
        {
            Error = Errors.OK;
            BFUK.PlaceBetsReq PlaceBetsReq = new BFUK.PlaceBetsReq();
            BFUK.PlaceBetsResp PlaceBetsResp = new BFUK.PlaceBetsResp();

            PlaceBetsReq.header = GetUKHeader();

            List<BFUK.PlaceBets> BetList = new List<BFUK.PlaceBets>();

            List<List<MultipleMarketBet>> BetsSortedByMarket = new List<List<MultipleMarketBet>>();

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
                TempBet.marketId = GetMarketByName(bets[i].MarketName);
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

        /// <summary>
        /// Cancel all bets on the predefined market. This method is slow and if you have the BetResult array of all the bets use the other CancleAllBets function instead
        /// </summary>
        /// <returns></returns>

        public bool CancelAllBets(String MarketName)
        {
            Error = Errors.OK;
            ErrorDetails = "";

            BetMUStatus[] BetStatus = GetCurrentUnmatchedBetStatus(MarketName);

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

        public BetMUStatus[] GetCurrentBetStatus(String MarketName)
        {
            return GetCurrentBetStatus('B', MarketName);
        }

        /// <summary>
        /// Get the current unmatched bet status
        /// </summary>
        /// <returns>Array of BetMUStatus</returns>

        public BetMUStatus[] GetCurrentUnmatchedBetStatus(String MarketName)
        {
            return GetCurrentBetStatus('U', MarketName);
        }

        private BetMUStatus[] GetCurrentBetStatus(Char MatchedUnmatched, String MarketName)
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
            GetMUBetsReq.marketId = GetMarketByName(MarketName);
            GetMUBetsReq.orderBy = BFUK.BetsOrderByEnum.BET_ID;
            GetMUBetsReq.recordCount = 200;
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

        public CurrentPL[] GetCurrentMatchedAndUnmatchedPL(String MarketName)
        {
            CurrentPL[] RetVal = GetCurrentMatchedPL(MarketName);
            
            BetMUStatus[] UnmatchedBets = GetCurrentBetStatus('U', MarketName);

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
        public CurrentPL[] GetCurrentMatchedPL(String MarketName)
        {
            return GetCurrentMatchedPL(false, MarketName);
        }

        public CurrentPL[] GetCurrentMatchedPL2(String MarketName, int[] selIds)
        {
            CurrentPL[] retVal = new CurrentPL[selIds.Count()];

            for (int i = 0; i < selIds.Count(); i++)
            {
                retVal[i] = new CurrentPL();
                retVal[i].selectionId = selIds[i];
                retVal[i].amount = 0;
            }

            BetMUStatus[] bets = GetCurrentBetStatus('M', MarketName);

            foreach (BetMUStatus bet in bets)
            {
                for (int i = 0; i < retVal.Count(); i++)
                {
                    if (bet.selectionId == retVal[i].selectionId)
                    {
                        if (bet.betType == 'B')
                            retVal[i].amount += ((bet.averagePriceMatched - 1) * bet.sizeMatched);
                        else
                            retVal[i].amount -= ((bet.averagePriceMatched - 1) * bet.sizeMatched);
                    }
                    if(bet.selectionId != retVal[i].selectionId)
                    {
                        if (bet.betType == 'B')
                            retVal[i].amount -= bet.sizeMatched;
                        else
                            retVal[i].amount += bet.sizeMatched;
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Get current profit and loss for matched bets with or without commision
        /// </summary>
        /// <param name="Commission">True if commision should be calculated it the proffit and loss</param>
        /// <returns>Array of CurrentPL</returns>

        public CurrentPL[] GetCurrentMatchedPL(bool Commission, String MarketName)
        {
            Error = Errors.OK;
            ErrorDetails = "";
            
            BFUK.GetMarketProfitAndLossReq GetMarketProfitAndLossReq = new BFUK.GetMarketProfitAndLossReq();
            BFUK.GetMarketProfitAndLossResp GetMarketProfitAndLossResp = new BFUK.GetMarketProfitAndLossResp();

            GetMarketProfitAndLossReq.marketID = GetMarketByName(MarketName);
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

        public static double OnePriceLower(double Price)
        {
            return BetFairLadder.OnePriceLower(Price);
            /*if (Price <= 2)
                return Price - 0.01d;
            else if (Price <= 3)
                return Price - 0.02d;
            else if (Price <= 4)
                return Price - 0.05d;
            else if (Price <= 6)
                return Price - 0.1d;
            else if (Price <= 10)
                return Price - 0.2d;
            else if (Price <= 20)
                return Price - 0.5d;
            else if (Price <= 30)
                return Price - 1d;
            else if (Price <= 50)
                return Price - 2d;
            else if (Price <= 100)
                return Price - 5d;
            else
                return Price - 10d;*/
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
            double lPrice = RoundPrice(price1, price1backOrLay);
            double hPrice = RoundPrice(price2, price2backOrLay);

            if (lPrice == hPrice)
                return 0;
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

            return retVal;
        }
    }
}
