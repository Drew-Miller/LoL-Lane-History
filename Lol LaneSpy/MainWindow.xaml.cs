using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ParserEngine;
using System.ComponentModel;
using System.Threading;
using System.Globalization;

namespace theInterface
{
    /// <summary>
    /// Provides in depth data of a summoner throughout the 
    /// league of legends game and in specific xp and gold data for laning phase
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        /// <todo>
        /// add labels
        /// 
        /// LoL Lane History isn't endorsed by Riot Games and doesn't 
        /// reflect the views or opinions of Riot Games or anyone officially involved 
        /// in producing or managing League of Legends. League of Legends and Riot Games 
        /// are trademarks or registered trademarks of 
        /// Riot Games, Inc. League of Legends © Riot Games, Inc.
        /// </todo>

        //strings to save summoner and api data
        private string sTBText = "";
        private string aTBText = "";
        private string sTBStatement = "Enter Summoner Name";
        private string aTBStatement = "Enter API Key";
        private List<Dictionary<string, string>> gameData = new List<Dictionary<string, string>>();

        public MainWindow()
        {
            InitializeComponent();

            regionCB.Items.Add("NA");
            regionCB.Items.Add("EUNE");
            regionCB.Items.Add("BR");
            regionCB.Items.Add("LAN");
            regionCB.Items.Add("TR");
            regionCB.Items.Add("EUW");
            regionCB.Items.Add("KR");
            regionCB.Items.Add("CN");
            regionCB.Items.Add("LAN");
            regionCB.Items.Add("RU");

            summonerTB.Text = sTBStatement;
            apiTB.Text = aTBStatement;
            pgBar.Visibility = Visibility.Collapsed;
        }

        //clears and sets the text box up for user input
        private void summonerTB_GotFocus(object sender, RoutedEventArgs e)
        {
            summonerTB.Text = sTBText;
        }

        private void apiTB_GotFocus(object sender, RoutedEventArgs e)
        {
            apiTB.Text = aTBText;
        }

        //if the textbox focus is lost, choose what to display
        private void summonerTB_LostFocus(object sender, RoutedEventArgs e)
        {
            sTBText = summonerTB.Text;

            if(sTBText == "")
            {
                summonerTB.Text = sTBStatement;
            }
        }

        private void apiTB_LostFocus(object sender, RoutedEventArgs e)
        {
            aTBText = apiTB.Text;

            if(aTBText == "")
            {
                apiTB.Text = aTBStatement;
            }
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            Parser p = new ParserGames();

            var myDict = new Dictionary<string, string>();

            myDict["r"] = regionCB.Text.ToLower();
            myDict["s"] = summonerTB.Text;
            myDict["a"] = apiTB.Text;

            if(p.validInput(myDict))
            {
                gamesTV.Items.Clear();
                resultsTB.Text = "Please wait. Riot's API has time request limits...";

                //show loading bar and hide all else
                pgBar.Visibility = Visibility.Visible;
                summonerTB.IsEnabled = false;
                apiTB.IsEnabled = false;
                goButton.IsEnabled = false;
                regionCB.IsEnabled = false;

                BackgroundWorker getData = new BackgroundWorker();
                getData.WorkerReportsProgress = true;
                getData.ProgressChanged += runLoadingBar;

                //what runs when worker is started
                getData.DoWork += delegate (object s, DoWorkEventArgs args)
                {
                    Thread t = new Thread(delegate ()
                    { 
                        try
                        {
                            p.run();
                        }
                        catch { }
                    });

                    t.Start();
                    
                    
                    //changes loading bar data
                    for (int i = 0; i <= 1000; i++)
                    {
                        //mostly for looks, but since the p.run() action is time based
                        //this actually is a way of going solving the problem, although not directly.
                        Thread.Sleep(33);
                        getData.ReportProgress(i);
                    }                   

                    //pauses until the thread is done
                    while ( t.IsAlive);

                    //collect data from the parser
                    gameData = p.results();
                };

                //after worker is done
                getData.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
                {
                    if (gameData.Count != 0)
                    {
                        resultsTB.Text = "";

                        foreach (Dictionary<string, string> game in gameData)
                        {
                            //stores the game and adds a header of the lane that was played into a treeviewitem
                            TreeViewItem tvItem = new TreeViewItem()
                            {
                                Header = (" " + game["matchId"] + " ").ToString(),
                                Height = 25,
                                Tag = game,
                                BorderBrush = Brushes.Black,
                                BorderThickness = new Thickness(1.0),
                                Padding = new Thickness(3.0)
                            };

                            //event for when the item gets clicked
                            tvItem.MouseLeftButtonUp += displayTVData;

                            gamesTV.Items.Add(tvItem);
                        }
                    }

                    else
                    {
                        resultsTB.Text = "There is no data retrieved from Riot Games.";
                    }

                    //hide loading bar
                    pgBar.Visibility = Visibility.Collapsed;
                    summonerTB.IsEnabled = true;
                    apiTB.IsEnabled = true;
                    goButton.IsEnabled = true;
                    regionCB.IsEnabled = true;
                };

                getData.RunWorkerAsync();
            }
        }

        private void displayTVData(object sender, MouseButtonEventArgs e)
        {
            //gets sender as tvitem and retrieves the game that was from the tag of the item
            TreeViewItem tvItem = sender as TreeViewItem;

            Dictionary<string, string> game = tvItem.Tag as Dictionary<string, string>;

            StringBuilder sb = new StringBuilder();

            //format text for resultsTB
            string champ = game["champion"].ToString();

            while(champ.Length < 12)
            {
                champ = (champ + " ").ToString();
            }

            string lane = game["lane"];

            string outcome = game["outcome"];

            if(outcome == "L")
            {
                outcome = "Defeat";
            }
            else
            {
                outcome = "Victory";
            }

            string kda = (game["kills"] + "/" + game["deaths"] + "/" + game["assists"]).ToString();
            string level = game["level"];

            string length = ("Time: " + game["length"]).ToString();

            string[] splits = length.Split(':');

            //if the seconds were in single digits
            if(splits[2].Length < 2)
            {
                length = (splits[0] + ":" + splits[1] + ":0" + splits[2]).ToString();
            }

            //basic statistics
            string fb = game["fb"].ToString();
            string fbA = game["fbA"].ToString();
            string cs = game["cs"].ToString();

            //statistics for the first ten minutes
            string xp10 = game["xpDiff10"].ToString();
            string dmg10 = game["dmg10"].ToString();
            string gold10 = game["goldDiff10"].ToString();
            string cs10 = game["csPer10"].ToString();

            //extras that might not have data if game under twenty minutes
            string xp20 = "";
            string dmg20 = "";
            string gold20 = "";
            string cs20 = "";

            //if past twenty
            if (game["twenty"] == true.ToString())
            {
                xp20 = game["xpDiff20"].ToString();
                dmg20 = game["dmg20"].ToString();
                gold20 = game["goldDiff20"].ToString();
                cs20 = game["csPer20"].ToString();
            }

            sb.Append(
                "Champion: " + champ + "Level: " + level + "\t" + lane + "\t\t" + outcome
                + Environment.NewLine + Environment.NewLine
                + "KDA:        " + kda + "\t\t\t\t\t" + length
                + Environment.NewLine + Environment.NewLine
                + "First Blood: " + fb + "\t\t\t\t\tAssist on FB: " + fbA
                + Environment.NewLine + Environment.NewLine
                + "CS: " + cs
                + Environment.NewLine + "CS/Min\t\t\t\tat 10: " + cs10 + "\t\tat 20: " + cs20
                + Environment.NewLine + Environment.NewLine
                + "Diff between Opponent DMG/min\tat 10: " + dmg10 + " \tat 20: " + dmg20
                + Environment.NewLine + Environment.NewLine
                + "Gold/Min\t\t\tat 10: " + gold10 + "\tat 20: " + gold20
                );

            resultsTB.Text = sb.ToString();
        }

        private void runLoadingBar(object sender, ProgressChangedEventArgs e)
        {
            pgBar.Value = e.ProgressPercentage;
        }

        private void api_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Hello! My application currently does not have an offical application" + Environment.NewLine
                +"Riot Games API key to access Riot Games API, so you will have to supply" + Environment.NewLine
                + "your own." + Environment.NewLine + Environment.NewLine 
                + "To get an API key you must have a League of Legends account," + Environment.NewLine
                + "and if you do go to " + Environment.NewLine +
                "https://developer.riotgames.com/" + Environment.NewLine
                + " to get your specific API key. If you do not have an account and wish" + Environment.NewLine
                + "to make one that can be used on the North American server, " + Environment.NewLine
                + "Head on over to " + Environment.NewLine + "https://signup.na.leagueoflegends.com/en/signup/index?realm_key=na" + Environment.NewLine
                + "In the mean time, I will leave my api key here, on 1/7/2016." + Environment.NewLine
                + "Do not rely on it since it might be deactivated in the near future." + Environment.NewLine
                + "My key is: 15fed3ea-10f9-41d7-a678-f7daf2659a1f. " + Environment.NewLine
                + "If you wish to give feed back or bug reports," + Environment.NewLine + "email me at d.m.lespaul@gmail.com"
                + Environment.NewLine + Environment.NewLine + Environment.NewLine);

            resultsTB.Text = sb.ToString();
        }

        private void help_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Hello! Here you may seek help on what information the application" + Environment.NewLine
                + "is providing you. This application provides basic data such as champion," + Environment.NewLine
                + "level at the end of the game, KDA and lane. Although these statistics are" + Environment.NewLine
                + "interesting, they are readily available in different forms on many different" + Environment.NewLine
                + "applications. This application is specifically geared towards lane statistics" + Environment.NewLine
                + "so you can be prepared if your opponent performs well on their champ," + Environment.NewLine
                + "or if they are a dud. Here are some break downs of the stats displayed:" + Environment.NewLine + Environment.NewLine
                + "First Blood: This stat is interesting because right off the bat, you can" + Environment.NewLine
                + "\tprepare yourself for a powerful levels 1 through 3 from your" + Environment.NewLine
                + "\topponent. The first blood assist stat kind of shows the same" + Environment.NewLine
                + "\tthing, but is mainly important if they are in the jungle, because it" + Environment.NewLine
                + "\tshows the ability to force FBs across the map." + Environment.NewLine + Environment.NewLine
                + "CS per minute: This statistic displays the average cs/minute" + Environment.NewLine
                + "\tof your opponent at the 10 minute mark and then the" + Environment.NewLine
                + "\t10 to 20 minute mark. This statistic can be used to" +Environment.NewLine
                + "\tfind out if your opponent farms like a bronze, or like Froggen" + Environment.NewLine + Environment.NewLine
                + "Gold per minute: Although usually has a strong correlation with cs per" + Environment.NewLine
                + "\tminute, this can also show towers taken by their team along with" + Environment.NewLine
                + "\tkills on the enemy in lane." + Environment.NewLine + Environment.NewLine
                + "Differentce between Opponent damage per minute:" + Environment.NewLine
                + "\tThis statistic shows how much more damage per minute that our" + Environment.NewLine
                + "\tsummoner is outputting over their opponents damage. This stat" + Environment.NewLine
                + "\tcan be nice to show how effective their bullying in lane is."
                + Environment.NewLine + Environment.NewLine + Environment.NewLine);

            resultsTB.Text = sb.ToString();
        }

        private void disclaimer_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("LoL Lane History isn't endorsed by Riot Games and doesn't" + Environment.NewLine 
                + "reflect the views or opinions of Riot Games or anyone officially involved" + Environment.NewLine
                + "in producing or managing League of Legends. League of Legends and" + Environment.NewLine
                + "Riot Games are trademarks or registered trademarks of" + Environment.NewLine
                + "Riot Games, Inc. League of Legends © Riot Games, Inc." + Environment.NewLine);

            resultsTB.Text = sb.ToString();
        }
    }
}
