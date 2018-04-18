using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NeuronDotNet.Core.Backpropagation;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Net.Sockets;

namespace New_T_Bot
{
    public partial class NewTBot_Form : Form
    {
        #region Variables

        BetFairApi BFAPI = new BetFairApi();
        MarketRunners[] MKTRunners = new MarketRunners[2];
        MarketPrices MKTPrices = new MarketPrices();
        Score CurrentScore = new Score(0, 0, 0, 0, 0, 0, 'A', false);
        Player PlayerA = new Player();
        Player PlayerB = new Player();
        List<Point> MatchSnapshot = new List<Point>();
        Market MarketSnapshot = new Market();
        Prices CurrentPrices = new Prices();
        Point CurrentPoint = new Point();
        TennisPrices.Prices TPrices = new TennisPrices.Prices();
        TennisPrices.VjekosPrices PricesToWrite = new TennisPrices.VjekosPrices();
        string Trader;
        double PLA = new double();
        double PLB = new double();

        bool Connected = false;
        ConnectingDialog ConnDialog;
        BackgroundWorker ConnectionWorker;
        private TcpListener tcpListener;
        private StreamReader reader;
        private Socket socket;
        private NetworkStream nStream;

        #endregion

        #region Scoring

        private Score IncreasePointA(Score ScoreToChange)
        {
            if (ScoreToChange.GamesA == 6 && ScoreToChange.GamesB == 6 && (ScoreToChange.SetsA + ScoreToChange.SetsB != (Convert.ToInt32(Sets_cb.Text)) - 1 || TiebreakLast_cb.Checked == true))
            {
                switch (ScoreToChange.PointA)//igra se tie break
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        ScoreToChange.PointA++;
                        if((ScoreToChange.PointA+ScoreToChange.PointB)%2!=0)
                        {
                            if (ScoreToChange.PlayerServing == 'A')
                            {
                                ScoreToChange.PlayerServing = 'B';
                            }
                            else
                            {
                                ScoreToChange.PlayerServing = 'A';
                            }
                        }
                        break;
                    default:
                        if ((ScoreToChange.PointA - ScoreToChange.PointB) > 0.5)
                        {
                            if ((ScoreToChange.PointA + ScoreToChange.PointB) % 4 == 0||(ScoreToChange.PointA + ScoreToChange.PointB) % 4 == 3)
                            {
                                if (ScoreToChange.PlayerServing == 'A')
                                {
                                    ScoreToChange.PlayerServing = 'B';
                                }
                                else
                                {
                                    ScoreToChange.PlayerServing = 'A';
                                }
                            }
                            ScoreToChange.PointA = 0;
                            ScoreToChange.PointB = 0;
                            ScoreToChange = IncreaseGameA(ScoreToChange);
                            break;
                        }
                        else
                        {
                            ScoreToChange.PointA++;
                            if ((ScoreToChange.PointA + ScoreToChange.PointB) % 2 != 0)
                            {
                                if (ScoreToChange.PlayerServing == 'A')
                                {
                                    ScoreToChange.PlayerServing = 'B';
                                }
                                else
                                {
                                    ScoreToChange.PlayerServing = 'A';
                                }
                            }
                            break;
                        }

                }
            }
            else
            {
                switch (ScoreToChange.PointA)//normalni gem
                {
                    case 0:
                    case 1:
                    case 2:
                        ScoreToChange.PointA++;
                        break;
                    case 3:
                        switch (ScoreToChange.PointB)
                        {
                            case 3:
                                ScoreToChange.PointA++;
                                break;
                            case 4:
                                ScoreToChange.PointB = 3;
                                break;
                            default:
                                ScoreToChange.PointA = 0;
                                ScoreToChange.PointB = 0;
                                ScoreToChange = IncreaseGameA(ScoreToChange);
                                if (ScoreToChange.PlayerServing == 'A')
                                {
                                    ScoreToChange.PlayerServing = 'B';
                                }
                                else
                                {
                                    ScoreToChange.PlayerServing = 'A';
                                }
                                break;
                        }
                        break;
                    case 4:
                        ScoreToChange.PointA = 0;
                        ScoreToChange.PointB = 0;
                        ScoreToChange = IncreaseGameA(ScoreToChange);
                        if (ScoreToChange.PlayerServing == 'A')
                        {
                            ScoreToChange.PlayerServing = 'B';
                        }
                        else
                        {
                            ScoreToChange.PlayerServing = 'A';
                        }
                        break;
                }
            }
            return ScoreToChange;
        }

        private Score DecreasePointA(Score ScoreToChange)
        {
            if (ScoreToChange.GamesA == 6 && ScoreToChange.GamesB == 6 && (ScoreToChange.SetsA + ScoreToChange.SetsB != (Convert.ToInt32(Sets_cb.Text)) - 1 || TiebreakLast_cb.Checked == true))
            {
                switch (ScoreToChange.PointA)//igra se tie break
                {
                    case 0:
                        ScoreToChange.PointA = 0;
                        break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        ScoreToChange.PointA--;
                        if ((ScoreToChange.PointA + ScoreToChange.PointB) % 2 != 0)
                        {
                            if (ScoreToChange.PlayerServing == 'A')
                            {
                                ScoreToChange.PlayerServing = 'B';
                            }
                            else
                            {
                                ScoreToChange.PlayerServing = 'A';
                            }
                        }
                        break;
                }
            }
            else
            {
                switch (ScoreToChange.PointA)//normalni gem
                {
                    case 0:
                        ScoreToChange.PointA = 0;
                        break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        ScoreToChange.PointA--;
                        break;
                }
            }
            return ScoreToChange;
        }

        private Score IncreasePointB(Score ScoreToChange)
        {
            if (ScoreToChange.GamesA == 6 && ScoreToChange.GamesB == 6 && (ScoreToChange.SetsA + ScoreToChange.SetsB != (Convert.ToInt32(Sets_cb.Text)) - 1 || TiebreakLast_cb.Checked == true))
            {
                switch (ScoreToChange.PointB)//igra se tie break
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        ScoreToChange.PointB++;
                        if ((ScoreToChange.PointA + ScoreToChange.PointB) % 2 != 0)
                        {
                            if (ScoreToChange.PlayerServing == 'A')
                            {
                                ScoreToChange.PlayerServing = 'B';
                            }
                            else
                            {
                                ScoreToChange.PlayerServing = 'A';
                            }
                        }
                        break;
                    default:
                        if ((ScoreToChange.PointB - ScoreToChange.PointA) > 0.5)
                        {
                            if ((ScoreToChange.PointA + ScoreToChange.PointB) % 4 == 0 || (ScoreToChange.PointA + ScoreToChange.PointB) % 4 == 3)
                            {
                                if (ScoreToChange.PlayerServing == 'A')
                                {
                                    ScoreToChange.PlayerServing = 'B';
                                }
                                else
                                {
                                    ScoreToChange.PlayerServing = 'A';
                                }
                            }
                            ScoreToChange.PointA = 0;
                            ScoreToChange.PointB = 0;
                            ScoreToChange = IncreaseGameB(ScoreToChange);
                            break;
                        }
                        else
                        {
                            ScoreToChange.PointB++;
                            if ((ScoreToChange.PointA + ScoreToChange.PointB) % 2 != 0)
                            {
                                if (ScoreToChange.PlayerServing == 'A')
                                {
                                    ScoreToChange.PlayerServing = 'B';
                                }
                                else
                                {
                                    ScoreToChange.PlayerServing = 'A';
                                }
                            }
                            break;
                        }

                }
            }
            else
            {
                switch (ScoreToChange.PointB)//normalni gem
                {
                    case 0:
                    case 1:
                    case 2:
                        ScoreToChange.PointB++;
                        break;
                    case 3:
                        switch (ScoreToChange.PointA)
                        {
                            case 3:
                                ScoreToChange.PointB = 4;
                                break;
                            case 4:
                                ScoreToChange.PointA = 3;
                                break;
                            default:
                                ScoreToChange.PointA = 0;
                                ScoreToChange.PointB = 0;
                                ScoreToChange = IncreaseGameB(ScoreToChange);
                                if (ScoreToChange.PlayerServing == 'A')
                                {
                                    ScoreToChange.PlayerServing = 'B';
                                }
                                else
                                {
                                    ScoreToChange.PlayerServing = 'A';
                                }
                                break;
                        }
                        break;
                    case 4:
                        ScoreToChange.PointA = 0;
                        ScoreToChange.PointB = 0;
                        ScoreToChange = IncreaseGameB(ScoreToChange);
                        if (ScoreToChange.PlayerServing == 'A')
                        {
                            ScoreToChange.PlayerServing = 'B';
                        }
                        else
                        {
                            ScoreToChange.PlayerServing = 'A';
                        }
                        break;
                }
            }
            return ScoreToChange;
        }

        private Score DecreasePointB(Score ScoreToChange)
        {
            if (ScoreToChange.GamesA == 6 && ScoreToChange.GamesB == 6 && (ScoreToChange.SetsA + ScoreToChange.SetsB != (Convert.ToInt32(Sets_cb.Text)) - 1 || TiebreakLast_cb.Checked == true))
            {
                switch (ScoreToChange.PointB)//igra se tie break
                {
                    case 0:
                        ScoreToChange.PointB = 0;
                        break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        ScoreToChange.PointB--;
                        if ((ScoreToChange.PointA + ScoreToChange.PointB) % 2 != 0)
                        {
                            if (ScoreToChange.PlayerServing == 'A')
                            {
                                ScoreToChange.PlayerServing = 'B';
                            }
                            else
                            {
                                ScoreToChange.PlayerServing = 'A';
                            }
                        }
                        break;
                }
            }
            else
            {
                switch (ScoreToChange.PointB)//normalni gem
                {
                    case 0:
                        ScoreToChange.PointB = 0;
                        break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        ScoreToChange.PointB--;
                        break;
                }
            }
            return ScoreToChange;
        }

        private Score IncreaseGameA(Score ScoreToChange)
        {
            if (ScoreToChange.GamesA < 5)
            {
                ScoreToChange.GamesA++;
            }
            else
            {
                switch (ScoreToChange.GamesA)
                {
                    case 5:
                        switch (ScoreToChange.GamesB)
                        {
                            case 5:
                            case 6:
                                ScoreToChange.GamesA++;
                                break;
                            default:
                                ScoreToChange.GamesA = 0;
                                ScoreToChange.GamesB = 0;
                                ScoreToChange = IncreaseSetA(ScoreToChange);
                                break;
                        }
                        break;
                    default:
                        if (ScoreToChange.SetsA + ScoreToChange.SetsB != (Convert.ToInt32(Sets_cb.Text)) - 1)
                        {
                            ScoreToChange.GamesA = 0;
                            ScoreToChange.GamesB = 0;
                            ScoreToChange = IncreaseSetA(ScoreToChange);
                            break;
                        }
                        else
                        {
                            if (TiebreakLast_cb.Checked == true)
                            {
                                ScoreToChange.GamesA = 0;
                                ScoreToChange.GamesB = 0;
                                ScoreToChange = IncreaseSetA(ScoreToChange);
                                break;
                            }
                            if ((ScoreToChange.GamesA - ScoreToChange.GamesB) > 0.5)
                            {
                                ScoreToChange.GamesA = 0;
                                ScoreToChange.GamesB = 0;
                                ScoreToChange = IncreaseSetA(ScoreToChange);
                                break;
                            }
                            else
                            {
                                ScoreToChange.GamesA++;
                            }
                        }
                        break;
                }
            }
            return ScoreToChange;
        }

        private Score DecreaseGameA(Score ScoreToChange)
        {
            ScoreToChange.GamesA--;

            if (ScoreToChange.GamesA < 0)
            {
                ScoreToChange.GamesA = 0;
            }
            if (ScoreToChange.PlayerServing == 'A')
            {
                ScoreToChange.PlayerServing = 'B';
            }
            else
            {
                ScoreToChange.PlayerServing = 'A';
            }

            return ScoreToChange;
        }

        private Score IncreaseGameB(Score ScoreToChange)
        {
            if (ScoreToChange.GamesB < 5)
            {
                ScoreToChange.GamesB++;
            }
            else
            {
                switch (ScoreToChange.GamesB)
                {
                    case 5:
                        switch (ScoreToChange.GamesA)
                        {
                            case 5:
                            case 6:
                                ScoreToChange.GamesB++;
                                break;
                            default:
                                ScoreToChange.GamesA = 0;
                                ScoreToChange.GamesB = 0;
                                ScoreToChange = IncreaseSetB(ScoreToChange);
                                break;
                        }
                        break;
                    default:
                        if (ScoreToChange.SetsA + ScoreToChange.SetsB != (Convert.ToInt32(Sets_cb.Text)) - 1)
                        {
                            ScoreToChange.GamesA = 0;
                            ScoreToChange.GamesB = 0;
                            ScoreToChange = IncreaseSetB(ScoreToChange);
                            break;
                        }
                        else
                        {
                            if (TiebreakLast_cb.Checked == true)
                            {
                                ScoreToChange.GamesA = 0;
                                ScoreToChange.GamesB = 0;
                                ScoreToChange = IncreaseSetB(ScoreToChange);
                                break;
                            }
                            if ((ScoreToChange.GamesB - ScoreToChange.GamesB) > 0.5)
                            {
                                ScoreToChange.GamesA = 0;
                                ScoreToChange.GamesB = 0;
                                ScoreToChange = IncreaseSetB(ScoreToChange);
                                break;
                            }
                            else
                            {
                                ScoreToChange.GamesB++;
                            }
                        }
                        break;
                }
            }
            return ScoreToChange;
        }

        private Score DecreaseGameB(Score ScoreToChange)
        {
            ScoreToChange.GamesB--;

            if (ScoreToChange.GamesB < 0)
            {
                ScoreToChange.GamesB = 0;
            }
            if (ScoreToChange.PlayerServing == 'A')
            {
                ScoreToChange.PlayerServing = 'B';
            }
            else
            {
                ScoreToChange.PlayerServing = 'A';
            }
            return ScoreToChange;
        }

        private Score IncreaseSetA(Score ScoreToChange)
        {
            ScoreToChange.SetsA++;

            if (ScoreToChange.SetsA > Convert.ToInt32(Sets_cb.Text) / 2)
            {
                ScoreToChange.SetsA = Convert.ToInt32(Sets_cb.Text) / 2;
            }

            return ScoreToChange;
        }

        private Score DecreaseSetA(Score ScoreToChange)
        {
            ScoreToChange.SetsA--;

            if (ScoreToChange.SetsA < 0)
            {
                ScoreToChange.SetsA = 0;
            }

            return ScoreToChange;
        }

        private Score IncreaseSetB(Score ScoreToChange)
        {
            ScoreToChange.SetsB++;

            if (ScoreToChange.SetsB > Convert.ToInt32(Sets_cb.Text) / 2)
            {
                ScoreToChange.SetsB = Convert.ToInt32(Sets_cb.Text) / 2;
            }

            return ScoreToChange;
        }

        private Score DecreaseSetB(Score ScoreToChange)
        {
            ScoreToChange.SetsB--;

            if (ScoreToChange.SetsB < 0)
            {
                ScoreToChange.SetsB = 0;
            }

            return ScoreToChange;
        }

        private void WritingGuiScores(Score ScoreToWrite) //Upisuje tekuci rezultat u GUI
        {
            if (ScoreToWrite.GamesA == 6 && ScoreToWrite.GamesB == 6 && (ScoreToWrite.SetsA + ScoreToWrite.SetsB != (Convert.ToInt32(Sets_cb.Text)) - 1 || TiebreakLast_cb.Checked == true))
            {
                PointsA_tb.Text = ScoreToWrite.PointA.ToString();
                PointsB_tb.Text = ScoreToWrite.PointB.ToString();
                if (ScoreToWrite.PlayerServing == 'A')
                {
                    PlayerAServing_rb.Checked = true;
                    PlayerBServing_rb.Checked = false;
                }
                else
                {
                    PlayerAServing_rb.Checked = false;
                    PlayerBServing_rb.Checked = true;
                }
                GamesA_tb.Text = ScoreToWrite.GamesA.ToString();
                GamesB_tb.Text = ScoreToWrite.GamesB.ToString();
                SetsA_tb.Text = ScoreToWrite.SetsA.ToString();
                SetsB_tb.Text = ScoreToWrite.SetsB.ToString();
            }
            else
            {
                switch (ScoreToWrite.PointA)
                {
                    case 0:
                        PointsA_tb.Text = ScoreToWrite.PointA.ToString();
                        break;
                    case 1:
                        PointsA_tb.Text = "15";
                        break;
                    case 2:
                        PointsA_tb.Text = "30";
                        break;
                    case 3:
                        PointsA_tb.Text = "40";
                        break;
                    case 4:
                        PointsA_tb.Text = "adv";
                        break;
                }
                switch (ScoreToWrite.PointB)
                {
                    case 0:
                        PointsB_tb.Text = ScoreToWrite.PointB.ToString();
                        break;
                    case 1:
                        PointsB_tb.Text = "15";
                        break;
                    case 2:
                        PointsB_tb.Text = "30";
                        break;
                    case 3:
                        PointsB_tb.Text = "40";
                        break;
                    case 4:
                        PointsB_tb.Text = "adv";
                        break;
                }
                if (ScoreToWrite.PlayerServing == 'A')
                {
                    PlayerAServing_rb.Checked = true;
                    PlayerBServing_rb.Checked = false;
                }
                else
                {
                    PlayerAServing_rb.Checked = false;
                    PlayerBServing_rb.Checked = true;
                }
                GamesA_tb.Text = ScoreToWrite.GamesA.ToString();
                GamesB_tb.Text = ScoreToWrite.GamesB.ToString();
                SetsA_tb.Text = ScoreToWrite.SetsA.ToString();
                SetsB_tb.Text = ScoreToWrite.SetsB.ToString();
            }

        }
        #endregion


        public NewTBot_Form()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");

            //FOR CONNECTING TO MOBILE PHONE
            ConnectionWorker = new BackgroundWorker();
            ConnectionWorker.DoWork += new DoWorkEventHandler(ConnectionWorkerDoWork);
            ConnectionWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ConnectionWorkerFinished);
            ConnectionWorker.WorkerSupportsCancellation = true;

            InitializeComponent();
            Login LoginForm = new Login();
            LoginForm.ShowDialog();
            if (LoginForm.CloseForm)
            {
                Trader=LoginForm.username;
                LoginForm.Close();
            }
            Market_dg.Rows.Add();
            Market_dg.Rows.Add();
            Market_dg.Rows.Add();
            Market_dg.Rows[3].Height = 30;
            Market_dg.Rows[0].DefaultCellStyle.Font = new Font(Market_dg.Font, FontStyle.Bold);
            Market_dg.Rows[2].DefaultCellStyle.Font = new Font(Market_dg.Font, FontStyle.Bold);
        }

        private void GetMarket_bt_Click(object sender, EventArgs e)
        {
            BFAPI.LoginNonFree(Settings.username, Settings.password);
            BFAPI.GraphicMarketSelect(BetFairApi.Sports.Tennis);
            try
            {
                MKTRunners = BFAPI.GetMarketRunners();
                CurrentPL[] CurrentPaL = BFAPI.GetCurrentMatchedPL();
                PLA = CurrentPaL[0].amount;
                PLB = CurrentPaL[1].amount;
                PlayerA_tb.Text = MKTRunners[0].RunnerName;
                PlayerB_tb.Text = MKTRunners[1].RunnerName;
                PlayerA.Name = MKTRunners[0].RunnerName;
                PlayerA.selectionID = MKTRunners[0].SelectionId;
                PlayerB.Name = MKTRunners[1].RunnerName;
                PlayerB.selectionID = MKTRunners[1].SelectionId;
                label10.Text = "To back " + MKTRunners[0].RunnerName;
                label11.Text = "To back " + MKTRunners[1].RunnerName;
                label9.Text = "To back " + MKTRunners[0].RunnerName;
                label12.Text = "To back " + MKTRunners[1].RunnerName;

                BackgroundWorker RefreshMarket = new BackgroundWorker();
                RefreshMarket.DoWork += new DoWorkEventHandler(RefreshMarket_DoWork);
                RefreshMarket.WorkerSupportsCancellation = true;
                RefreshMarket.RunWorkerAsync((object)BFAPI.GetMarketId());
            }
            catch
            {
                MessageBox.Show("You haven't selected correct market. Please try again");
            }          
        }

        public delegate void RefreshMarkets();
        //public delegate void RefreshEdgeA();
        //public delegate void RefreshEdgeB();
        void RefreshMarket_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            int MarketID = (int)e.Argument;
            BetFairApi NewBF = new BetFairApi();
            MarketPrices CurrentMKTPrices = new MarketPrices();
            MarketPrices MirrorMKTPrices=new MarketPrices();

            NewBF.LoginNonFree(Settings.username, Settings.password);
            NewBF.ManualySetMarketId(MarketID);

            while (worker.CancellationPending == false)
            {
                /*RefreshEdgeA InstanceEdgeA = new RefreshEdgeA(delegate()
                    {
                        double EdgeA = new double();

                        EdgeA = (1 / Convert.ToDouble(Market_dg.Rows[0].Cells[2].Value) - 1 / Convert.ToDouble(OurPriceA_tb.Text)) * 100;
                        EdgeA = Math.Round(EdgeA, 2);

                        EdgeA_tb.Text = EdgeA.ToString() + "%";
                    }
                );
                 RefreshEdgeB InstanceEdgeB = new RefreshEdgeB(delegate()
                    {
                        double EdgeB = new double();

                        EdgeB = (1 / Convert.ToDouble(Market_dg.Rows[2].Cells[2].Value) - 1 / Convert.ToDouble(OurPriceB_tb.Text)) * 100;
                        EdgeB = Math.Round(EdgeB, 2);

                        EdgeB_tb.Text = EdgeB.ToString() + "%";
                    }
                );*/

                RefreshMarkets Instance = new RefreshMarkets(delegate()
                    {
                        CurrentMKTPrices = new MarketPrices();
                        try
                        {
                            CurrentMKTPrices = NewBF.GetMarketPrices3();
                        }
                        catch
                        {
                            MessageBox.Show(NewBF.ErrorDetails.ToString());
                        }
                        List<double> BackA = new List<double>();
                        List<double> LayA = new List<double>();
                        List<double> BackB = new List<double>();
                        List<double> LayB = new List<double>();

                        /*for (int br = 0; br <3; br++)
                        {
                            BackA.Add(CurrentMKTPrices.RunnerInformation[0].BackPrices[br].Price);
                            BackA.Add(BetFairApi.GetPrice(CurrentMKTPrices.RunnerInformation[1].LayPrices[br].Price / (CurrentMKTPrices.RunnerInformation[1].LayPrices[br].Price-1),'l'));                           
                        }
                        BackA.Sort();
                        for (int br = 1; br < BackA.Count(); br++)
                        {
                            if (BackA[br] == BackA[br - 1])
                            {
                                BackA.RemoveAt(br);
                                br--;
                            }
                        }
                        BackA.Reverse();

                        for (int br = 0; br < 3; br++)
                        {
                            LayA.Add(CurrentMKTPrices.RunnerInformation[0].LayPrices[br].Price);
                            LayA.Add(BetFairApi.GetPrice(CurrentMKTPrices.RunnerInformation[1].BackPrices[br].Price / (CurrentMKTPrices.RunnerInformation[1].BackPrices[br].Price - 1), 'l'));
                        }
                        LayA.Sort();
                        for (int br = 1; br < LayA.Count(); br++)
                        {
                            if (LayA[br] == LayA[br - 1])
                            {
                                LayA.RemoveAt(br);
                                br--;
                            }
                        }

                        for (int br = 0; br < 3; br++)
                        {
                            BackB.Add(CurrentMKTPrices.RunnerInformation[1].BackPrices[br].Price);
                            BackB.Add(BetFairApi.GetPrice(CurrentMKTPrices.RunnerInformation[0].LayPrices[br].Price / (CurrentMKTPrices.RunnerInformation[0].LayPrices[br].Price - 1), 'l'));
                        }
                        BackB.Sort();
                        for (int br = 1; br < BackB.Count(); br++)
                        {
                            if (BackB[br] == BackB[br - 1])
                            {
                                BackB.RemoveAt(br);
                                br--;
                            }
                        }
                        BackB.Reverse();

                        for (int br = 0; br < 3; br++)
                        {
                            LayB.Add(CurrentMKTPrices.RunnerInformation[1].LayPrices[br].Price);
                            LayB.Add(BetFairApi.GetPrice(CurrentMKTPrices.RunnerInformation[0].BackPrices[br].Price / (CurrentMKTPrices.RunnerInformation[0].BackPrices[br].Price - 1), 'l'));
                        }
                        LayB.Sort();
                        for (int br = 1; br < LayB.Count(); br++)
                        {
                            if (LayB[br] == LayB[br - 1])
                            {
                                LayB.RemoveAt(br);
                                br--;
                            }
                        }*/
                        //do mirroring
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < CurrentMKTPrices.RunnerInformation[i].BackPrices.Count(); j++)
                            {
                                Market_dg.Rows[i*2].Cells[2 - j].Value = CurrentMKTPrices.RunnerInformation[i].BackPrices[j].Price; 
                            }
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < CurrentMKTPrices.RunnerInformation[i].BackPrices.Count(); j++)
                            {
                                Market_dg.Rows[i * 2+1].Cells[2 - j].Value = CurrentMKTPrices.RunnerInformation[i].BackPrices[j].AvaiableAmmount;
                            }
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < CurrentMKTPrices.RunnerInformation[i].LayPrices.Count(); j++)
                            {
                                Market_dg.Rows[i*2].Cells[3 + j].Value = CurrentMKTPrices.RunnerInformation[i].LayPrices[j].Price;
                            }
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < CurrentMKTPrices.RunnerInformation[i].LayPrices.Count(); j++)
                            {
                                Market_dg.Rows[i * 2+1].Cells[3 + j].Value = CurrentMKTPrices.RunnerInformation[i].LayPrices[j].AvaiableAmmount;
                            }
                        }
                    }
                );
                try
                {
                    Market_dg.Invoke(Instance);
                }
                catch
                {
                    MessageBox.Show("Match finished");
                    worker.CancelAsync();
                }
                //EdgeA_tb.Invoke(InstanceEdgeA);
                //EdgeB_tb.Invoke(InstanceEdgeB);
                System.Threading.Thread.Sleep(500);
            }
        }


        private void ChangeServe_bt_Click(object sender, EventArgs e)
        {
            if (PlayerAServing_rb.Checked)
            {
                PlayerAServing_rb.Checked = false;
                CurrentScore.PlayerServing = 'B';
                PlayerBServing_rb.Checked = true;
            }
            else
            {
                PlayerBServing_rb.Checked = false;
                CurrentScore.PlayerServing = 'A';
                PlayerAServing_rb.Checked = true;
            }
        }

        private void StakesAUp_bt_Click(object sender, EventArgs e)
        {
            StakesA_tb.Text = (Convert.ToDouble(StakesA_tb.Text) + 50).ToString();
        }

        private void StakesADown_bt_Click(object sender, EventArgs e)
        {
            StakesA_tb.Text = (Convert.ToDouble(StakesA_tb.Text) - 50).ToString();
            if (Convert.ToDouble(StakesA_tb.Text) <= 0)
            {
                StakesA_tb.Text = "0";
            }
        }

        private void StakesBUp_bt_Click(object sender, EventArgs e)
        {
            StakesB_tb.Text = (Convert.ToDouble(StakesB_tb.Text) + 50).ToString();
        }

        private void StakesBDown_bt_Click(object sender, EventArgs e)
        {
            StakesB_tb.Text = (Convert.ToDouble(StakesB_tb.Text) - 50).ToString();
            if (Convert.ToDouble(StakesB_tb.Text) <= 0)
            {
                StakesB_tb.Text = "0";
            }
        }

        private void PointsAUp_bt_Click(object sender, EventArgs e)
        {
            IncreasePointA(CurrentScore);
            WritingGuiScores(CurrentScore);
        }

        private void PointsBUp_bt_Click(object sender, EventArgs e)
        {
            IncreasePointB(CurrentScore);
            WritingGuiScores(CurrentScore);
        }

        private void SetsADown_bt_Click(object sender, EventArgs e)
        {
            DecreaseSetA(CurrentScore);
            WritingGuiScores(CurrentScore);
        }

        private void SetsAUp_bt_Click(object sender, EventArgs e)
        {
            IncreaseSetA(CurrentScore);
            WritingGuiScores(CurrentScore);
        }

        private void SetsBUp_bt_Click(object sender, EventArgs e)
        {
            IncreaseSetB(CurrentScore);
            WritingGuiScores(CurrentScore);
        }

        private void SetsBdown_bt_Click(object sender, EventArgs e)
        {
            DecreaseSetB(CurrentScore);
            WritingGuiScores(CurrentScore);
        }

        private void GamesAUp_bt_Click(object sender, EventArgs e)
        {
            IncreaseGameA(CurrentScore);
            WritingGuiScores(CurrentScore);
        }

        private void GamesADown_bt_Click(object sender, EventArgs e)
        {
            DecreaseGameA(CurrentScore);
            WritingGuiScores(CurrentScore);
        }

        private void GamesBUp_bt_Click(object sender, EventArgs e)
        {
            IncreaseGameB(CurrentScore);
            WritingGuiScores(CurrentScore);
        }

        private void GamesBDown_bt_Click(object sender, EventArgs e)
        {
            DecreaseGameB(CurrentScore);
            WritingGuiScores(CurrentScore);
        }

        private void PointsADown_bt_Click(object sender, EventArgs e)
        {
            DecreasePointA(CurrentScore);
            WritingGuiScores(CurrentScore);
        }

        private void PointsBDown_bt_Click(object sender, EventArgs e)
        {
            DecreasePointB(CurrentScore);
            WritingGuiScores(CurrentScore);
        }

        private void DisplayBets()
        {
            double OurPriceA = new double();
            double OurPriceB = new double();
            double OurPriceAAwins = new double();
            double OurPriceBAwins = new double();
            double OurPriceABwins = new double();
            double OurPriceBBwins = new double();
            double OurPriceAWMBack = new double();
            double OurPriceAWMLay = new double();
            double OurPriceBWMBack = new double();
            double OurPriceBWMLay = new double();
            double OurExtremeBackWM = new double();
            double OurExtremeLayWM = new double();
            char AwinsBettingOn = new char();
            char BwinsBettingOn = new char();


            OurPriceA = 1/Convert.ToDouble(OurPriceA_tb.Text);
            OurPriceB = 1/Convert.ToDouble(OurPriceB_tb.Text);
            OurPriceAAwins = 1 / Convert.ToDouble(OurPriceAAwins_tb.Text);
            OurPriceBAwins = 1 / Convert.ToDouble(OurPriceBAwins_tb.Text);
            OurPriceABwins = 1 / Convert.ToDouble(OurPriceABwins_tb.Text);
            OurPriceBBwins = 1 / Convert.ToDouble(OurPriceBBwins_tb.Text);

            OurPriceAWMBack = 1 / (OurPriceA - Convert.ToDouble(MarginToBackA_tb.Text) / 100);
            OurPriceAWMBack = Math.Round(OurPriceAWMBack, 3);
            OurPriceAWMLay = 1 / (OurPriceA + Convert.ToDouble(MarginToBackB_tb.Text) / 100);
            OurPriceAWMLay = Math.Round(OurPriceAWMLay, 3);

            OurPriceBWMBack = 1 / (OurPriceB - Convert.ToDouble(MarginToBackB_tb.Text) / 100);
            OurPriceBWMBack = Math.Round(OurPriceBWMBack, 3);
            OurPriceBWMLay = 1 / (OurPriceB + Convert.ToDouble(MarginToBackA_tb.Text) / 100);
            OurPriceBWMLay = Math.Round(OurPriceBWMLay, 3);

            if (OurPriceAAwins >= 0.5)// A favourite when he wins point
            {
                OurExtremeLayWM = 1 / (OurPriceAAwins + Convert.ToDouble(MarginToBackB_tb.Text) / 100);
                OurExtremeLayWM = Math.Round(OurExtremeLayWM, 3);
                AwinsBettingOn = 'A';
            }
            else
            {
                OurExtremeBackWM = 1 / (OurPriceBAwins - Convert.ToDouble(MarginToBackB_tb.Text) / 100);
                OurExtremeBackWM = Math.Round(OurExtremeBackWM, 3);
                AwinsBettingOn = 'B';
            }

            if (OurPriceABwins >= 0.5)// A favourite when he wins point
            {
                OurExtremeBackWM = 1 / (OurPriceABwins - Convert.ToDouble(MarginToBackA_tb.Text) / 100);
                OurExtremeBackWM = Math.Round(OurExtremeBackWM, 3);
                BwinsBettingOn = 'A';
            }
            else
            {
                OurExtremeLayWM = 1 / (OurPriceBBwins + Convert.ToDouble(MarginToBackA_tb.Text) / 100);
                OurExtremeLayWM = Math.Round(OurExtremeLayWM, 3);
                BwinsBettingOn = 'B';
            }

            if (OurPriceA >= 0.5)
            {
                NextBets_dg.Rows.Clear();
                //NextBets_dg.Rows.Add((BwinsBettingOn == 'A') ? PlayerA.Name : PlayerB.Name, (BwinsBettingOn == 'A') ? 'B' : 'L', (BwinsBettingOn == 'A') ? (OurExtremeBackWM).ToString() : (OurExtremeLayWM).ToString(), (BwinsBettingOn == 'A') ? StakesA_tb.Text : StakesB_tb.Text);
                NextBets_dg.Rows.Add(PlayerA.Name, 'B', (OurPriceAWMBack).ToString(), StakesA_tb.Text);
                NextBets_dg.Rows.Add(PlayerA.Name, 'L', (OurPriceAWMLay).ToString(), StakesB_tb.Text);
                //NextBets_dg.Rows.Add(PlayerA.Name, 'L', (OurExtremeLayWM).ToString(), StakesB_tb.Text);
                //zakomentirani extreme betovi
            }
            else
            {
                NextBets_dg.Rows.Clear();
                //NextBets_dg.Rows.Add((AwinsBettingOn == 'A') ? PlayerA.Name : PlayerB.Name, (AwinsBettingOn == 'A') ? 'L' : 'B', (AwinsBettingOn == 'A') ? (OurExtremeLayWM).ToString() : (OurExtremeBackWM).ToString(), (AwinsBettingOn == 'A') ? StakesB_tb.Text : StakesA_tb.Text);
                NextBets_dg.Rows.Add(PlayerB_tb.Text, 'B', (OurPriceBWMBack).ToString(), StakesB_tb.Text);
                NextBets_dg.Rows.Add(PlayerB_tb.Text, 'L', (OurPriceBWMLay).ToString(), StakesA_tb.Text);
                //NextBets_dg.Rows.Add(PlayerB_tb.Text, 'L', (OurExtremeLayWM).ToString(), StakesA_tb.Text);
                //zakomentirani extreme betovi
            }
        }

        private void OurPriceA_tb_TextChanged(object sender, EventArgs e)
        {
            DisplayBets();
            CalculateEdge();
        }

        private void OurPriceB_tb_TextChanged(object sender, EventArgs e)
        {
            DisplayBets();
            CalculateEdge();
        }

        private void GetMarketBets()
        {
            BetMUStatus[] Unmatched = BFAPI.GetCurrentUnmatchedBetStatus();

            BetsOnMarket_dg.Rows.Clear();

            foreach (BetMUStatus Item in Unmatched)
            {
                BetsOnMarket_dg.Rows.Add((Item.selectionId == PlayerA.selectionID) ? PlayerA.Name : PlayerB.Name, Item.betType, Item.priceRequested.ToString(), Item.sizeUnmatched.ToString(), true, Item.betId);
            }
            CurrentPL[] PL = BFAPI.GetCurrentMatchedPL();
            PLA_tb.Text = PL[0].amount.ToString();
            PLB_tb.Text = PL[1].amount.ToString();

        }

        private void GetMarketBets_bt_Click(object sender, EventArgs e)
        {
            GetMarketBets();
        }

        private void CancelBets_bt_Click(object sender, EventArgs e)
        {
            List<BetResults> BetsToCancel=new List<BetResults>();
            
            for (int i = 0; i < BetsOnMarket_dg.Rows.Count; i++)
            {
                if (!BetsOnMarket_dg.Rows[i].IsNewRow)
                {
                    if (BetsOnMarket_dg.Rows[i].Cells[4].Value.ToString() == "True")
                    {
                        BetsToCancel.Add(new BetResults());
                        BetsToCancel[BetsToCancel.Count-1].betId=Convert.ToInt64(BetsOnMarket_dg.Rows[i].Cells[5].Value);
                    }
                }
            }
            BFAPI.CancelAllBets(BetsToCancel.ToArray());
            GetMarketBets();
        }

        private void PlaceBets()
        {
            List<Bet> BetsToPlace = new List<Bet>();

            for (int i = 0; i < NextBets_dg.Rows.Count-1; i++)
            {
                Bet BetToAdd = new Bet();
                BetToAdd.selectionId = (NextBets_dg.Rows[i].Cells[0].Value.ToString() == PlayerA.Name) ? PlayerA.selectionID : PlayerB.selectionID;
                BetToAdd.backOrLay = Convert.ToChar(NextBets_dg.Rows[i].Cells[1].Value);
                BetToAdd.price = Convert.ToDouble(NextBets_dg.Rows[i].Cells[2].Value);
                BetToAdd.amount=Convert.ToDouble(NextBets_dg.Rows[i].Cells[3].Value);
                if (BetToAdd.price > 1 && BetToAdd.price < 1001)
                {
                    BetsToPlace.Add(BetToAdd);
                }
            }
            if (BetsAllowed_cb.Checked == true)
            {
                BetResults[] PlacedBets = BFAPI.PlaceBet(BetsToPlace.ToArray());
            }
        }

        private void PlaceBetsWins(char PlayerWon)
        {
            double OurPriceA = new double();
            double OurPriceB = new double();
            double OurPriceAWMBack = new double();
            double OurPriceAWMLay = new double();
            double OurPriceBWMBack = new double();
            double OurPriceBWMLay = new double();

            List<Bet> BetsToPlace = new List<Bet>();

            if (PlayerWon == 'A')
            {
                OurPriceA = 1 / Convert.ToDouble(OurPriceAAwins_tb.Text);
                OurPriceB = 1 / Convert.ToDouble(OurPriceBAwins_tb.Text);
            }
            else
            {
                OurPriceA = 1 / Convert.ToDouble(OurPriceABwins_tb.Text);
                OurPriceB = 1 / Convert.ToDouble(OurPriceBBwins_tb.Text);
            }
            

            OurPriceAWMBack = 1 / (OurPriceA - Convert.ToDouble(MarginToBackA_tb.Text) / 100);
            OurPriceAWMBack = Math.Round(OurPriceAWMBack, 3);
            OurPriceAWMLay = 1 / (OurPriceA + Convert.ToDouble(MarginToBackB_tb.Text) / 100);
            OurPriceAWMLay = Math.Round(OurPriceAWMLay, 3);

            OurPriceBWMBack = 1 / (OurPriceB - Convert.ToDouble(MarginToBackB_tb.Text) / 100);
            OurPriceBWMBack = Math.Round(OurPriceBWMBack, 3);
            OurPriceBWMLay = 1 / (OurPriceB + Convert.ToDouble(MarginToBackA_tb.Text) / 100);
            OurPriceBWMLay = Math.Round(OurPriceBWMLay, 3);

            if (OurPriceA >= 0.5)
            {
                if (OurPriceA == 1)
                {
                    Bet BetToAddwins = new Bet();
                    BetToAddwins.selectionId = PlayerA.selectionID;
                    BetToAddwins.backOrLay = 'B';
                    BetToAddwins.price = 1.01;
                    BetToAddwins.amount = 1000;
                    BetsToPlace.Add(BetToAddwins);
                    BetResults[] PlacedBets = BFAPI.PlaceBet(BetsToPlace.ToArray());
                    return;
                }
                Bet BetToAddB = new Bet();
                BetToAddB.selectionId = PlayerA.selectionID;
                BetToAddB.backOrLay = 'B';
                BetToAddB.price = Convert.ToDouble(OurPriceAWMBack.ToString());
                BetToAddB.amount = Convert.ToDouble(StakesA_tb.Text);
                if (BetToAddB.price > 1 && BetToAddB.price < 1001)
                {
                    BetsToPlace.Add(BetToAddB);
                }

                Bet BetToAddL = new Bet();
                BetToAddL.selectionId = PlayerA.selectionID;
                BetToAddL.backOrLay = 'L';
                BetToAddL.price = Convert.ToDouble(OurPriceAWMLay.ToString());
                BetToAddL.amount = Convert.ToDouble(StakesB_tb.Text);
                if (BetToAddL.price > 1 && BetToAddL.price < 1001)
                {
                    BetsToPlace.Add(BetToAddL);
                }

                if (BetsAllowed_cb.Checked == true)
                {
                    BetResults[] PlacedBets = BFAPI.PlaceBet(BetsToPlace.ToArray());
                }
            }
            else
            {
                if (OurPriceB == 1)
                {
                    Bet BetToAddwins = new Bet();
                    BetToAddwins.selectionId = PlayerB.selectionID;
                    BetToAddwins.backOrLay = 'B';
                    BetToAddwins.price = 1.01;
                    BetToAddwins.amount = 1000;
                    BetsToPlace.Add(BetToAddwins);
                    BetResults[] PlacedBets = BFAPI.PlaceBet(BetsToPlace.ToArray());
                    return;
                }
                Bet BetToAddB = new Bet();
                BetToAddB.selectionId = PlayerB.selectionID;
                BetToAddB.backOrLay = 'B';
                BetToAddB.price = Convert.ToDouble(OurPriceBWMBack.ToString());
                BetToAddB.amount = Convert.ToDouble(StakesB_tb.Text);
                if (BetToAddB.price > 1 && BetToAddB.price < 1001)
                {
                    BetsToPlace.Add(BetToAddB);
                }

                Bet BetToAddL = new Bet();
                BetToAddL.selectionId = PlayerB.selectionID;
                BetToAddL.backOrLay = 'L';
                BetToAddL.price = Convert.ToDouble(OurPriceBWMLay.ToString());
                BetToAddL.amount = Convert.ToDouble(StakesA_tb.Text);
                if (BetToAddL.price > 1 && BetToAddL.price < 1001)
                {
                    BetsToPlace.Add(BetToAddL);
                }

                if (BetsAllowed_cb.Checked == true)
                {
                    BetResults[] PlacedBets = BFAPI.PlaceBet(BetsToPlace.ToArray());
                }
            }
        }

        private void GetPL_bt_Click(object sender, EventArgs e)
        {
            CurrentPL[] PL = BFAPI.GetCurrentMatchedPL();
            PLA_tb.Text = PL[0].amount.ToString();
            PLB_tb.Text = PL[1].amount.ToString();
        }

        private void OurPriceAAwinsUp_bt_Click(object sender, EventArgs e)
        {
            OurPriceAAwins_tb.Text = BetFairApi.OnePriceHigher(Convert.ToDouble(OurPriceAAwins_tb.Text)).ToString();
        }

        private void OurPriceAAwinsDown_bt_Click(object sender, EventArgs e)
        {
            OurPriceAAwins_tb.Text = BetFairApi.OnePriceLower(Convert.ToDouble(OurPriceAAwins_tb.Text)).ToString();
        }

        private void OurPriceBAwinsUp_bt_Click(object sender, EventArgs e)
        {
            OurPriceBAwins_tb.Text = BetFairApi.OnePriceHigher(Convert.ToDouble(OurPriceBAwins_tb.Text)).ToString();
        }

        private void OurPriceBAwinsDown_bt_Click(object sender, EventArgs e)
        {
            OurPriceBAwins_tb.Text = BetFairApi.OnePriceLower(Convert.ToDouble(OurPriceBAwins_tb.Text)).ToString();
        }

        private void OurPriceABwinsUp_bt_Click(object sender, EventArgs e)
        {
            OurPriceABwins_tb.Text = BetFairApi.OnePriceHigher(Convert.ToDouble(OurPriceABwins_tb.Text)).ToString();
        }

        private void OurPriceABwinsDown_bt_Click(object sender, EventArgs e)
        {
            OurPriceABwins_tb.Text = BetFairApi.OnePriceLower(Convert.ToDouble(OurPriceABwins_tb.Text)).ToString();
        }

        private void OurPriceBBwinsUp_bt_Click(object sender, EventArgs e)
        {
            OurPriceBBwins_tb.Text = BetFairApi.OnePriceHigher(Convert.ToDouble(OurPriceBBwins_tb.Text)).ToString();
        }

        private void OurPriceBBwinsDown_bt_Click(object sender, EventArgs e)
        {
            OurPriceBBwins_tb.Text = BetFairApi.OnePriceLower(Convert.ToDouble(OurPriceBBwins_tb.Text)).ToString();
        }

        private void TakeMarketSnapshot()
        {
            List<double> BackAPrice=new List<double>();
            List<double> LayAPrice = new List<double>();
            List<double> BackBPrice = new List<double>();
            List<double> LayBPrice = new List<double>();
            List<double> BackAAmount = new List<double>();
            List<double> LayAAmount = new List<double>();
            List<double> BackBAmount = new List<double>();
            List<double> LayBAmount = new List<double>();
            MarketPrices CurrentMKTPrices=BFAPI.GetMarketPrices3();

            BackAPrice.Add(Convert.ToDouble(Market_dg.Rows[0].Cells[2].Value));
            BackAPrice.Add(Convert.ToDouble(Market_dg.Rows[0].Cells[1].Value));
            BackAPrice.Add(Convert.ToDouble(Market_dg.Rows[0].Cells[0].Value));

            LayAPrice.Add(Convert.ToDouble(Market_dg.Rows[0].Cells[3].Value));
            LayAPrice.Add(Convert.ToDouble(Market_dg.Rows[0].Cells[4].Value));
            LayAPrice.Add(Convert.ToDouble(Market_dg.Rows[0].Cells[5].Value));

            BackAAmount.Add(Convert.ToDouble(Market_dg.Rows[1].Cells[2].Value));
            BackAAmount.Add(Convert.ToDouble(Market_dg.Rows[1].Cells[1].Value));
            BackAAmount.Add(Convert.ToDouble(Market_dg.Rows[1].Cells[0].Value));

            LayAAmount.Add(Convert.ToDouble(Market_dg.Rows[1].Cells[3].Value));
            LayAAmount.Add(Convert.ToDouble(Market_dg.Rows[1].Cells[4].Value));
            LayAAmount.Add(Convert.ToDouble(Market_dg.Rows[1].Cells[5].Value));

            BackBPrice.Add(Convert.ToDouble(Market_dg.Rows[2].Cells[2].Value));
            BackBPrice.Add(Convert.ToDouble(Market_dg.Rows[2].Cells[1].Value));
            BackBPrice.Add(Convert.ToDouble(Market_dg.Rows[2].Cells[0].Value));

            LayBPrice.Add(Convert.ToDouble(Market_dg.Rows[2].Cells[3].Value));
            LayBPrice.Add(Convert.ToDouble(Market_dg.Rows[2].Cells[4].Value));
            LayBPrice.Add(Convert.ToDouble(Market_dg.Rows[2].Cells[5].Value));

            BackBAmount.Add(Convert.ToDouble(Market_dg.Rows[3].Cells[2].Value));
            BackBAmount.Add(Convert.ToDouble(Market_dg.Rows[3].Cells[1].Value));
            BackBAmount.Add(Convert.ToDouble(Market_dg.Rows[3].Cells[0].Value));

            LayBAmount.Add(Convert.ToDouble(Market_dg.Rows[3].Cells[3].Value));
            LayBAmount.Add(Convert.ToDouble(Market_dg.Rows[3].Cells[4].Value));
            LayBAmount.Add(Convert.ToDouble(Market_dg.Rows[3].Cells[5].Value));

            /*BackAPrice.Add(CurrentMKTPrices.RunnerInformation[0].BackPrices[0].Price);
            BackAPrice.Add(CurrentMKTPrices.RunnerInformation[0].BackPrices[1].Price);
            BackAPrice.Add(CurrentMKTPrices.RunnerInformation[0].BackPrices[2].Price);

            LayAPrice.Add(CurrentMKTPrices.RunnerInformation[0].LayPrices[0].Price);
            LayAPrice.Add(CurrentMKTPrices.RunnerInformation[0].LayPrices[1].Price);
            LayAPrice.Add(CurrentMKTPrices.RunnerInformation[0].LayPrices[2].Price);

            BackAAmount.Add(CurrentMKTPrices.RunnerInformation[0].BackPrices[0].AvaiableAmmount);
            BackAAmount.Add(CurrentMKTPrices.RunnerInformation[0].BackPrices[1].AvaiableAmmount);
            BackAAmount.Add(CurrentMKTPrices.RunnerInformation[0].BackPrices[2].AvaiableAmmount);

            LayAAmount.Add(CurrentMKTPrices.RunnerInformation[0].LayPrices[0].AvaiableAmmount);
            LayAAmount.Add(CurrentMKTPrices.RunnerInformation[0].LayPrices[1].AvaiableAmmount);
            LayAAmount.Add(CurrentMKTPrices.RunnerInformation[0].LayPrices[2].AvaiableAmmount);

            BackBPrice.Add(CurrentMKTPrices.RunnerInformation[1].BackPrices[0].Price);
            BackBPrice.Add(CurrentMKTPrices.RunnerInformation[1].BackPrices[1].Price);
            BackBPrice.Add(CurrentMKTPrices.RunnerInformation[1].BackPrices[2].Price);

            LayBPrice.Add(CurrentMKTPrices.RunnerInformation[1].LayPrices[0].Price);
            LayBPrice.Add(CurrentMKTPrices.RunnerInformation[1].LayPrices[1].Price);
            LayBPrice.Add(CurrentMKTPrices.RunnerInformation[1].LayPrices[2].Price);

            BackBAmount.Add(CurrentMKTPrices.RunnerInformation[1].BackPrices[0].AvaiableAmmount);
            BackBAmount.Add(CurrentMKTPrices.RunnerInformation[1].BackPrices[1].AvaiableAmmount);
            BackBAmount.Add(CurrentMKTPrices.RunnerInformation[1].BackPrices[2].AvaiableAmmount);

            LayBAmount.Add(CurrentMKTPrices.RunnerInformation[1].LayPrices[0].AvaiableAmmount);
            LayBAmount.Add(CurrentMKTPrices.RunnerInformation[1].LayPrices[1].AvaiableAmmount);
            LayBAmount.Add(CurrentMKTPrices.RunnerInformation[1].LayPrices[2].AvaiableAmmount);*/

            MarketSnapshot.BackAPrice = BackAPrice;
            MarketSnapshot.LayAPrice = LayAPrice;
            MarketSnapshot.BackBPrice = BackBPrice;
            MarketSnapshot.LayBPrice = LayBPrice;
            MarketSnapshot.BackAAmount = BackAAmount;
            MarketSnapshot.LayAAmount = LayAAmount;
            MarketSnapshot.BackBAmount = BackBAmount;
            MarketSnapshot.LayBAmount = LayBAmount;

        }

        #region Point Buttons
        private void Ace_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            Point NewPoint = new Point(CurrentScore, "AC", (PlayerAServing_rb.Checked) ? 'A' : 'B', MarketSnapshot);
            MatchSnapshot.Add(NewPoint);
            //posalji tonciju Point
            DoWorkAfterPoint(NewPoint);
            if (PlayerAServing_rb.Checked)
            {
                PointsAUp_bt_Click(null, null);
            }
            else
            {
                PointsBUp_bt_Click(null, null);
            }
        }

        private void WinnerServe_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            Point NewPoint = new Point(CurrentScore, "WS", (PlayerAServing_rb.Checked) ? 'A' : 'B', MarketSnapshot);
            MatchSnapshot.Add(NewPoint);
            //posalji tonciju Point
            DoWorkAfterPoint(NewPoint);
            if (PlayerAServing_rb.Checked)
            {
                PointsAUp_bt_Click(null, null);
            }
            else
            {
                PointsBUp_bt_Click(null, null);
            }
        }

        private void DblFault_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            Point NewPoint = new Point(CurrentScore, "DF", (PlayerAServing_rb.Checked) ? 'B' : 'A', MarketSnapshot);
            MatchSnapshot.Add(NewPoint);
            //posalji tonciju Point
            DoWorkAfterPoint(NewPoint);
            if (PlayerAServing_rb.Checked)
            {
                PointsBUp_bt_Click(null, null);
            }
            else
            {
                PointsAUp_bt_Click(null, null);
            }
        }

        private void WinnerPointA_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            Point NewPoint = new Point(CurrentScore, "WP", 'A', MarketSnapshot);
            MatchSnapshot.Add(NewPoint);
            //posalji tonciju Point
            DoWorkAfterPoint(NewPoint);
            PointsAUp_bt_Click(null, null);
        }

        private void WinnerPointB_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            Point NewPoint = new Point(CurrentScore, "WP", 'B', MarketSnapshot);
            MatchSnapshot.Add(NewPoint);
            //posalji tonciju Point
            DoWorkAfterPoint(NewPoint);
            PointsBUp_bt_Click(null, null);
        }

        private void ForcedErrorA_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            Point NewPoint = new Point(CurrentScore, "FE", 'A', MarketSnapshot);
            MatchSnapshot.Add(NewPoint);
            //posalji tonciju Point
            DoWorkAfterPoint(NewPoint);
            PointsAUp_bt_Click(null, null);
        }

        private void ForcedErrorB_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            Point NewPoint = new Point(CurrentScore, "FE", 'B', MarketSnapshot);
            MatchSnapshot.Add(NewPoint);
            //posalji tonciju Point 
            DoWorkAfterPoint(NewPoint);
            PointsBUp_bt_Click(null, null);
        }

        private void UnforcedErrorA_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            Point NewPoint = new Point(CurrentScore, "UE", 'A', MarketSnapshot);
            MatchSnapshot.Add(NewPoint);
            //posalji tonciju Point 
            DoWorkAfterPoint(NewPoint);
            PointsAUp_bt_Click(null, null);
        }

        private void UnforcedErrorB_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            Point NewPoint = new Point(CurrentScore, "UE", 'B', MarketSnapshot);
            MatchSnapshot.Add(NewPoint);
            //posalji tonciju Point 
            DoWorkAfterPoint(NewPoint);
            PointsBUp_bt_Click(null, null);
        }

        private void ShitErrorA_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            Point NewPoint = new Point(CurrentScore, "SE", 'A', MarketSnapshot);
            MatchSnapshot.Add(NewPoint);
            //posalji tonciju Point 
            DoWorkAfterPoint(NewPoint);
            PointsAUp_bt_Click(null, null);
        }

        private void ShitErrorB_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            Point NewPoint = new Point(CurrentScore, "SE", 'B', MarketSnapshot);
            MatchSnapshot.Add(NewPoint);
            //posalji tonciju Point 
            DoWorkAfterPoint(NewPoint);
            PointsBUp_bt_Click(null, null);
        }

        private void NeutralA_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            Point NewPoint = new Point(CurrentScore, "N", 'A', MarketSnapshot);
            MatchSnapshot.Add(NewPoint);
            //posalji tonciju Point
            DoWorkAfterPoint(NewPoint);
            PointsAUp_bt_Click(null, null);
        }

        private void NeutralB_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            Point NewPoint = new Point(CurrentScore, "N", 'B', MarketSnapshot);
            MatchSnapshot.Add(NewPoint);
            //posalji tonciju Point
            DoWorkAfterPoint(NewPoint);
            PointsBUp_bt_Click(null, null);
        }

        #endregion

        private void MarginsAUp_bt_Click(object sender, EventArgs e)
        {
            MarginToBackA_tb.Text = (Convert.ToDouble(MarginToBackA_tb.Text) + 0.5).ToString();
        }

        private void MarginsADown_bt_Click(object sender, EventArgs e)
        {
            MarginToBackA_tb.Text = (Convert.ToDouble(MarginToBackA_tb.Text) - 0.5).ToString();
        }

        private void MarginsBUp_bt_Click(object sender, EventArgs e)
        {
            MarginToBackB_tb.Text = (Convert.ToDouble(MarginToBackB_tb.Text) + 0.5).ToString();
        }

        private void MarginsBDown_bt_Click(object sender, EventArgs e)
        {
            MarginToBackB_tb.Text = (Convert.ToDouble(MarginToBackB_tb.Text) - 0.5).ToString();
        }

        private void MarginToBackA_tb_TextChanged(object sender, EventArgs e)
        {
            DisplayBets();
        }

        private void MarginToBackB_tb_TextChanged(object sender, EventArgs e)
        {
            DisplayBets();
        }

        private void StakesA_tb_TextChanged(object sender, EventArgs e)
        {
            DisplayBets();
        }

        private void StakesB_tb_TextChanged(object sender, EventArgs e)
        {
            DisplayBets();
        }

        private void Start_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            CurrentPoint.BFMarket = MarketSnapshot;
            CurrentPoint.CurrentScore = CurrentScore;
            CurrentPoint.PlayerWon = 'A';
            CurrentPoint.Type = "WS";
            TPrices = new TennisPrices.Prices();
            try
            {
                if (Surface_tb.Text=="")
                {
                    MessageBox.Show("You havent entered surface");
                }
                else if (Trader=="")
                {
                    MessageBox.Show("You havent entered trader");
                }
                else if (ATPWTA_cb.Text == "")   
                {
                    MessageBox.Show("You havent entered male or female");
                }
                else if (Sets_cb.Text == "")
                {
                    MessageBox.Show("You havent entered number of sets");
                }
                else
                {
                    int BFMKTID = BFAPI.GetMarketId();
                    PricesToWrite = TPrices.Init(Trader, Surface_tb.Text, ATPWTA_cb.Text, Convert.ToInt32(Sets_cb.Text) / 2 + 1, TiebreakLast_cb.Checked,BFMKTID, PlayerA.selectionID, PlayerB.selectionID, CurrentPoint.CopyToVP());
                }
            }
            catch
            {
                MessageBox.Show("Could not initialise math. Please select market");
            }
            OurPriceAAwins_tb.Text = Math.Round(PricesToWrite.PriceAAwins, 2).ToString();
            OurPriceBAwins_tb.Text = Math.Round(PricesToWrite.PriceBAwins, 2).ToString();
            OurPriceABwins_tb.Text = Math.Round(PricesToWrite.PriceABwins, 2).ToString();
            OurPriceBBwins_tb.Text = Math.Round(PricesToWrite.PriceBBwins, 2).ToString();
            OurPriceA_tb.Text = Math.Round(PricesToWrite.PriceA, 2).ToString();
            OurPriceB_tb.Text = Math.Round(PricesToWrite.PriceB, 2).ToString();
        }

        private void DoWorkAfterPoint(Point PointToAdd)
        {
            PricesToWrite = TPrices.Calc(PointToAdd.CopyToVP());
            OurPriceAAwins_tb.Text = Math.Round(PricesToWrite.PriceAAwins, 2).ToString();
            OurPriceBAwins_tb.Text = Math.Round(PricesToWrite.PriceBAwins, 2).ToString();
            OurPriceABwins_tb.Text = Math.Round(PricesToWrite.PriceABwins, 2).ToString();
            OurPriceBBwins_tb.Text = Math.Round(PricesToWrite.PriceBBwins, 2).ToString();
            OurPriceA_tb.Text = Math.Round(PricesToWrite.PriceA, 2).ToString();
            OurPriceB_tb.Text = Math.Round(PricesToWrite.PriceB, 2).ToString();
            //PlaceBets();
            GetMarketBets_bt_Click(null, null);
        }

        private void Undo_bt_Click(object sender, EventArgs e)
        {
            char UndoServer = new char();
            TPrices.Undo();
            CurrentScore = MatchSnapshot[MatchSnapshot.Count - 1].CurrentScore;
            UndoServer = CurrentScore.PlayerServing;
            WritingGuiScores(CurrentScore);
            if (UndoServer == 'A')
            {
                PlayerAServing_rb.Checked = true;
                PlayerBServing_rb.Checked = false;
                CurrentScore.PlayerServing = 'A';
            }
            else
            {
                PlayerAServing_rb.Checked = false;
                PlayerBServing_rb.Checked = true;
                CurrentScore.PlayerServing = 'B';
            }
            MatchSnapshot.RemoveAt(MatchSnapshot.Count - 1);

        }

        private void CalculateEdge()
        {
            double EdgeA = new double();
            double EdgeB = new double();

            EdgeA = (1 / MarketSnapshot.BackAPrice[0] - 1 / Convert.ToDouble(OurPriceA_tb.Text))*100;
            EdgeA = Math.Round(EdgeA, 2);
            EdgeB = (1 / MarketSnapshot.BackBPrice[0] - 1 / Convert.ToDouble(OurPriceB_tb.Text))*100;
            EdgeB = Math.Round(EdgeB, 2);

            EdgeA_tb.Text = EdgeA.ToString() + "%";
            EdgeB_tb.Text = EdgeB.ToString() + "%";
        }

        private void CalculateEdgeB()
        {
            double EdgeB = new double();

            EdgeB = (1 / MarketSnapshot.BackBPrice[0] - 1 / Convert.ToDouble(OurPriceB_tb.Text)) * 100;
            EdgeB = Math.Round(EdgeB, 2);

            EdgeB_tb.Text = EdgeB.ToString() + "%";
        }

        private void PLA_tb_TextChanged(object sender, EventArgs e)
        {
            if (Convert.ToDouble(PLA_tb.Text) < (PLA-75))
            {
                MarginsADown_bt_Click(null, null);
                MarginsADown_bt_Click(null, null);
            }
            else if (Convert.ToDouble(PLA_tb.Text) > (PLA+75))
            {
                MarginsAUp_bt_Click(null, null);
                MarginsAUp_bt_Click(null, null);
            }
            PLA = Convert.ToDouble(PLA_tb.Text);
        }

        private void PLB_tb_TextChanged(object sender, EventArgs e)
        {
            if (Convert.ToDouble(PLB_tb.Text) < (PLB - 75))
            {
                MarginsBDown_bt_Click(null, null);
                MarginsBDown_bt_Click(null, null);
            }
            else if (Convert.ToDouble(PLB_tb.Text) > (PLB + 75))
            {
                MarginsBUp_bt_Click(null, null);
                MarginsBUp_bt_Click(null, null);
            }
            PLB = Convert.ToDouble(PLB_tb.Text);
        }

        private void Fault_bt_Click(object sender, EventArgs e)
        {
            TakeMarketSnapshot();
            Point NewPoint = new Point(CurrentScore, "FA", (PlayerAServing_rb.Checked) ? 'B' : 'A', MarketSnapshot);
            MatchSnapshot.Add(NewPoint);
            //posalji tonciju Point
            DoWorkAfterPoint(NewPoint);
        }

        private void Connect_bt_Click(object sender, EventArgs e)
        {
            ConnDialog = new ConnectingDialog();
            ConnectionWorker.RunWorkerAsync();
            ConnDialog.Show();
            ConnDialog.FormClosed += new FormClosedEventHandler(ConnDialog_FormClosed);
        }

        private void Recconect_bt_Click(object sender, EventArgs e)
        {
            tcpListener.Stop();
            socket.Close();
        }

        public void ConnDialog_FormClosed(object sender, EventArgs e)
        {
            tcpListener.Stop();
        }

        public void ConnectionWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            Connected = false;
            DialogResult Sniper = new DialogResult();
            SniperSelection SniperSelection = new SniperSelection();
            Sniper = SniperSelection.ShowDialog();
            if (Sniper == DialogResult.Yes)
            {
                tcpListener = new TcpListener(System.Net.IPAddress.Parse("0.0.0.0"), 20365);
            }
            else
            {
                tcpListener = new TcpListener(System.Net.IPAddress.Parse("0.0.0.0"), 20364);//5-IVA 4-PETRA
            }

            /*if (SniperSelector == DialogResult.Yes)
            {
                tcpListener = new TcpListener(System.Net.IPAddress.Parse("0.0.0.0"), 20365);//5-IVA 4-PETRA
            }
            else
            {
                tcpListener = new TcpListener(System.Net.IPAddress.Parse("0.0.0.0"), 20364);
            }*/
            SniperSelection.Hide();
            tcpListener.Start();
            try
            {
                e.Result = (Object)tcpListener.AcceptSocket();
            }
            catch
            {
                e.Cancel = true;
            }
        }

        public void ConnectionWorkerFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                ConnDialog.Close();
            }
            catch { }
            if (e.Cancelled == true)
            {
                socket = null;
                Connect_bt.Enabled = true;
                Recconect_bt.Enabled = false;
                Conn_lbl.Text = "Not connected";
                Conn_lbl.BackColor = Color.Magenta;
            }
            else
            {
                Connected = true;
                Conn_lbl.Text = "Connected";
                Conn_lbl.BackColor = Color.Green;
                socket = (Socket)e.Result;
                nStream = new NetworkStream(socket);
                reader = new StreamReader(nStream);
                Connect_bt.Enabled = false;
                Recconect_bt.Enabled = true;
                BackgroundWorker Reader = new BackgroundWorker();
                Reader.DoWork += new DoWorkEventHandler(Reader_DoWork);
                Reader.WorkerSupportsCancellation = true;
                Reader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Reader_RunWorkerCompleted);
                Reader.RunWorkerAsync();
            }
            ConnDialog.Dispose();
            ConnectionWorker.Dispose();
        }

        private void Reader_DoWork(object o, DoWorkEventArgs e)
        {
            if (Connected == false)
                return;
            while (true)
            {
                try
                {
                    Char input = reader.ReadLine()[0];
                    WorkInput(input);
                }
                catch
                {
                    Console.WriteLine("DISCONECTED");
                    e.Cancel = true;
                    Connected = false;
                    break;
                }
            }
        }
        private void Reader_RunWorkerCompleted(object o, RunWorkerCompletedEventArgs e)
        {
            ConnDialog = new ConnectingDialog();
            ConnectionWorker.RunWorkerAsync();
            ConnDialog.Show();
            ConnDialog.FormClosed += new FormClosedEventHandler(ConnDialog_FormClosed);
        }

        public delegate void Radi();
        public void WorkInput(Char Input)
        {
            Radi posao = new Radi(delegate()
            {
                switch (Input)
                {
                    case 'A':
                        CancelBets_bt_Click(null, null);
                        PlaceBetsWins('A');
                        GetMarketBets_bt_Click(null, null);
                        MessageBox.Show(PlayerA_tb.Text + " won the point");
                        break;
                    case 'D':
                        CancelBets_bt_Click(null, null);
                        PlaceBetsWins('B');
                        GetMarketBets_bt_Click(null, null);
                        MessageBox.Show(PlayerB_tb.Text + " won the point");
                        break;
                    case '*':
                        BetsAllowed_cb.Checked = false;
                        MessageBox.Show("Wrong score, please change");
                        System.Media.SystemSounds.Beep.Play();
                        Undo_bt_Click(null, null);
                        break;
                    case '#':
                        BetsAllowed_cb.Checked = !BetsAllowed_cb.Checked;
                        break;
                    case 'S':
                        CancelBets_bt_Click(null, null);
                        MessageBox.Show("Bets Off");
                        break;

                }
            });
            this.Invoke(posao);
        }
       

        /*private void Market_dg_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                CalculateEdge();
            }
        }*/

    }
}
