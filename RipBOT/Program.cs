using System;
using System.Configuration;
using Discord;
using Discord.Commands;
using Google.Apis.YouTube;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using RipBOT.models;

namespace RipBOT
{
    class Program
    {
        private string YouTube = ConfigurationManager.AppSettings["YouTubeAPIKey"];
        private string connect = ConfigurationManager.AppSettings["connectionToken"];
        private CommandService cs;
        private DiscordClient _bot;

        static void Main(string[] args)
        {
            new Program().Start();
        }

        /// <summary>
        ///     Sets up discord server and commands. Starts bot and connects.
        /// </summary>
        public void Start()
        {
            // Set bot settings
            _bot = new DiscordClient(b =>
            {
                b.AppName = "Rip BOT";
                b.AppUrl = "https://discordapp.com/api";
                b.LogLevel = LogSeverity.Info;
                b.LogHandler = LogEvents;
            });

            // Set command settings
            _bot.UsingCommands(b =>
            {
                b.PrefixChar = '-';
                b.AllowMentionPrefix = true;
                b.HelpMode = HelpMode.Public;
            });

            // Create commands
            CreateCommands();

            // Launch bot asynchronously
            _bot.ExecuteAndWait(async () =>
            {
                await _bot.Connect(connect);
            });
        }

        /// <summary>
        ///     Creates all commands that bot will recognize using Discord.Commands
        /// </summary>
        public void CreateCommands()
        {
            cs = _bot.GetService<CommandService>();
            
            cs.CreateCommand("zie")
                .Description("Description of Meme Zie")
                .Do(async (e) => 
                {
                    await e.Channel.SendMessage("Zie's internet sucks. He's a meme <3");
                });
                        
            cs.CreateCommand("drzl")
                .Description("Description of Meme DRZL")
                .Do(async (e) => {
                    await e.Channel.SendMessage("DRZL doesn't know how to tag people. @DRZL#6122");
                });

            cs.CreateCommand("zest")
                .Description("Description of Meme Zest")
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage("Zest loves to bathe in his footstools and privilege");
                });

            cs.CreateCommand("shotyz")
                .Description("Description of Meme Shotyz")
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage("Shotyz can't edgeguard anything, should stick to Overwatch");
                });

            //TODO: Add "chKn"

            cs.CreateCommand("kappa") //TODO: Replace with -meme for memegenerator
                .Description("Displays Kappa")
                .Do(async (e) =>
                {
                    await e.Channel.SendFile("../../images/Kappa.jpg");
                });
            
            cs.CreateCommand("youtube")
                .Description("Shows first YouTube video from search")
                .Parameter("search", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string search = e.GetArg("search");
                    string url = "";

                    var service = new YouTubeService(new BaseClientService.Initializer()
                    {
                        ApiKey = YouTube,
                        ApplicationName = "Discord Rip Bot"
                    });
                    var list = service.Search.List("snippet");
                    list.Q = search;
                    list.Order = SearchResource.ListRequest.OrderEnum.Relevance;
                    list.MaxResults = 10;

                    try
                    {
                        var response = await list.ExecuteAsync();
                        var first = response.Items[0];
                        switch (first.Id.Kind)
                        {
                            //TODO: Be able to handle Playlists as well
                            case "youtube#video":
                                url = "http://www.youtube.com/watch?v=" + first.Id.VideoId;
                                break;
                            default:
                                url = "https://www.youtube.com/results?search_query=" + search.Replace(" ", "%20");
                                break;
                        }
                        await e.Channel.SendMessage(e.User.Mention + ", how's this video? " + url);
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage("No results found.");
                    }
                });

            cs.CreateCommand("erase")
                .Description("Erases last ## messages (1-100)")
                .Parameter("number", ParameterType.Required)
                .Do(async (e) =>
                {
                    if (e.User.ServerPermissions.Administrator == true)
                    {
                        int numOfMsgs = Convert.ToInt32(e.GetArg("number"));
                        if (numOfMsgs > 0 && numOfMsgs <= 100)
                        {
                            Message[] msgs = e.Channel.DownloadMessages(numOfMsgs + 1).Result; // Add one to remove the -erase typed from admin
                            await e.Channel.DeleteMessages(msgs);
                        }
                        else
                        {
                            await e.Channel.SendMessage(e.User.Mention + ", need a number between 1 and 100!");
                        }
                    }
                    else
                    {
                        await e.Channel.SendMessage(e.User.Mention + ", only server admins can do this command!");
                    }
                });

            cs.CreateCommand("info")
                .Description("Grabs info from Google on any topic")
                .Parameter("search", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    var search = e.GetArg("search");
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        await e.Channel.SendMessage(e.User.Mention + ", http://lmgtfy.com/?q=" + search.Replace(" ", "%20"));
                    }
                });
            
            cs.CreateCommand("ow")
                .Description("Gets overwatch stats for a player")
                .Parameter("battleNet", ParameterType.Required)
                .Parameter("type", ParameterType.Optional)
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage("Overwatch stats coming here soon!");
                });

            cs.CreateCommand("framedata")
                .Description("Gets frame data for a single Smash 4 character")
                .Parameter("type", ParameterType.Required)
                .Parameter("character", ParameterType.Unparsed)
                .Do(async (e) => 
                {
                    // Determine type to display
                    string currentType = "";
                    if (e.GetArg("type").ToLower() == "air")
                        currentType = "0";
                    else if (e.GetArg("type").ToLower() == "ground")
                        currentType = "1";
                    else if (e.GetArg("type").ToLower() == "special")
                        currentType = "2";
                    else if (e.GetArg("type").ToLower() == "throws")
                        currentType = "3";
                    
                    // Get Smash Character model
                    SmashCharacter smashChar = GetSmashCharacter(e.GetArg("character"));

                    if (smashChar != null) { 
                        // Get Character's Frame Data from Kurogane Hammer
                        string dataURL = ConfigurationManager.AppSettings["frameDataId"].Replace("CHARID", smashChar.CharId);
                        HttpWebRequest dataReq = MakeRequest(dataURL);
                        HttpWebResponse dataResp = (HttpWebResponse)dataReq.GetResponse();
                        Stream dataStream = dataResp.GetResponseStream();
                        StreamReader dataRead = new StreamReader(dataStream);
                        string json = dataRead.ReadToEnd();
                        CharacterFrameData fData = new CharacterFrameData(json);
                        dataResp.Close();
                        dataRead.Close();
                        
                        // TYPES FROM KUROGANE HAMMER
                        //      0: Aerial Moves
                        //      1: Ground Moves
                        //      2: Special Moves
                        //      3: Throw Moves
                        if (currentType != "")
                        {
                            await e.Channel.SendMessage(e.User.Mention + ", check your inbox! Frame Data provided by Kurogane Hammer (http://www.kuroganehammer.com)");
                            await e.User.SendMessage(smashChar.ImageURL);
                            await e.User.SendMessage(string.Format("**" + smashChar.Name + "'s Frame Data**"));
                            string output = "";
                            foreach (var move in fData.moves)
                            {
                                if (move.Type == currentType)
                                {
                                    if (output.Length > 1850)
                                    {
                                        await e.User.SendMessage(output);
                                        output = "";
                                    }
                                    // So weird that this ONLY works with this exact formatting, GG discord
                                    output += string.Format("__{0}__```HitBox Active: {1}\nFAF:           {2}\nBase DMG:      {3}\nLand Lag:      {4}\nAutoCancel:    {5}```",
                                    string.IsNullOrWhiteSpace(move.Name) ? "--" : move.Name,
                                    string.IsNullOrWhiteSpace(move.HitboxActive) ? "--" : move.HitboxActive.Replace("&gt;", ">").Replace("&lt;", "<"),
                                    string.IsNullOrWhiteSpace(move.FirstActionableFrame) ? "--" : move.FirstActionableFrame.Replace("&gt;", ">").Replace("&lt;", "<"),
                                    string.IsNullOrWhiteSpace(move.BaseDamage) ? "--" : move.BaseDamage.Replace("&gt;", ">").Replace("&lt;", "<"),
                                    string.IsNullOrWhiteSpace(move.LandingLag) ? "--" : move.LandingLag.Replace("&gt;", ">").Replace("&lt;", "<"),
                                    string.IsNullOrWhiteSpace(move.AutoCancel) ? "--" : move.AutoCancel).Replace("&gt;", ">").Replace("&lt;", "<");
                                }
                            }
                            await e.User.SendMessage(output);
                        }
                        else
                        {
                            await e.Channel.SendMessage(e.User.Mention + ", incorrect type! Use 'ground', 'air', 'special', or 'throws'");
                        }
                    }
                    else
                    {
                        await e.Channel.SendMessage("Count not find any data on " + e.GetArg("character") + "!");
                    }
                });

            cs.CreateCommand("movement")
                .Description("Gets movement data for a single Smash 4 character")
                .Parameter("character")
                .Do(async (e) =>
                {
                    // Get Smash Character model
                    SmashCharacter smashChar = GetSmashCharacter(e.GetArg("character"));
                    if (smashChar != null)
                    {
                        // Get Character's Frame Data from Kurogane Hammer
                        string moveURL = ConfigurationManager.AppSettings["movementId"].Replace("CHARID", smashChar.CharId);
                        HttpWebRequest moveReq = MakeRequest(moveURL);
                        HttpWebResponse moveResp = (HttpWebResponse)moveReq.GetResponse();
                        Stream moveStream = moveResp.GetResponseStream();
                        StreamReader moveRead = new StreamReader(moveStream);
                        string json = moveRead.ReadToEnd();
                        CharacterMovements CharMovements = new CharacterMovements(json);
                        moveResp.Close();
                        moveRead.Close();

                        // Output everything
                        await e.Channel.SendMessage(e.User.Mention + ", check your inbox! Movement data provided by Kurogane Hammer (http://www.kuroganehammer.com/)");
                        await e.User.SendMessage(smashChar.ImageURL);
                        await e.User.SendMessage("**" + smashChar.Name + "'s Movement Data**");
                        string output = "```";
                        foreach (CharacterMovement movement in CharMovements.movements)
                        {
                            if (output.Length > 1850)
                            {
                                await e.User.SendMessage(output + "```");
                                output = "```";
                            }
                            output += string.Format("{0,25} {1}\n", movement.Name + ":", movement.Value);
                        }
                        await e.User.SendMessage(output + "```");
                    }
                    else
                    {
                        await e.Channel.SendMessage("Could not find any movement data on " + e.GetArg("character"));
                    }
                });
        }

        /// <summary>
        ///     Logs events to the console client.
        ///     TODO: Write to log file
        /// </summary>
        public void LogEvents(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine("[{0}] [{1}] {2}", e.Severity, e.Source, e.Message);
        }

        /// <summary>
        ///     Returns a web request object with appropriate settings. Need URL to API or Controller for data
        /// </summary>
        private HttpWebRequest MakeRequest(string URL)
        {
            HttpWebRequest returned = (HttpWebRequest)WebRequest.Create(URL);
            returned.Method = "GET";
            returned.ContentType = "application/json";
            return returned;
        }

        /// <summary>
        ///     Returns a populated model of a character by name from API. If returned object is null, then no such character
        /// </summary>
        private SmashCharacter GetSmashCharacter(string character)
        {
            try
            {
                // Get Character's ID from Kurogane Hammer
                string charURL = ConfigurationManager.AppSettings["charSmash4"].Replace("CHARNAME", character);
                HttpWebRequest charReq = MakeRequest(charURL);
                HttpWebResponse charResp = (HttpWebResponse)charReq.GetResponse();
                Stream charStream = charResp.GetResponseStream();
                StreamReader charRead = new StreamReader(charStream);
                string json = charRead.ReadToEnd();
                SmashCharacter smashChar = new SmashCharacter(json);
                charResp.Close();
                charRead.Close();
                return smashChar;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
