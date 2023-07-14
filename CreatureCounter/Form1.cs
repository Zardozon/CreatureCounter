using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
//using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CreatureCounter
{
    public partial class Form1 : Form
    {
        public string connString;
        public bool continueExecuting = false;
        public string dir = @"C:\Users\Trogdor\SotN-Randomizer";
        public string args = "/C node randomize -vvv -f presets/lycanthrope.json -s "; // 123";
        public string resultText = "";
        public int currentSeed = 0;
        public int startSeed = 0;
        public int stopSeed = 0;
        public int totalSeeds = 0;

        public DateTime startTime;
        public DateTime stopTime;

        //make a dictionary that holds separate groups of locations depending on preset name
        public string[] lycanthropeLocations = 
        {
            "Soul of Bat", 
            "Fire of Bat", 
            "Echo of Bat",
            "Force of Echo",
            "Soul of Wolf",
            "Power of Wolf",
            "Skill of Wolf",
            "Form of Mist",
            "Power of Mist",
            "Gas Cloud",
            "Cube of Zoe",
            "Spirit Orb",
            "Gravity Boots",
            "Leap Stone",
            "Holy Symbol",
            "Faerie Scroll",
            "Jewel of Open",
            "Merman Statue",
            "Bat Card",
            "Ghost Card",
            "Faerie Card",
            "Demon Card",
            "Sword Card",
            "Heart of Vlad",
            "Tooth of Vlad",
            "Rib of Vlad",
            "Ring of Vlad",
            "Eye of Vlad",
            "Spike Breaker",
            "Gold ring",
            "Silver ring",
            "Holy glasses",
            "Trio",
            "Ring of Arcana",
            "Mormegil",
            "Dark Blade",
            "Crystal cloak",
            "Sprite Card",
            "Nosedevil Card"
        };

        //phase this out
        //public string[] usefulRelics = 
        //{
        //    "Soul of Bat",
        //    "Form of Mist",
        //    "Power of Mist",
        //    "Gravity Boots",
        //    "Leap Stone",
        //    "Jewel of Open",
        //    "Heart of Vlad",
        //    "Tooth of Vlad",
        //    "Rib of Vlad",
        //    "Ring of Vlad",
        //    "Eye of Vlad",
        //    "Spike Breaker",
        //    "Gold ring",
        //    "Silver ring",
        //    "Holy glasses"
        //};

        //make a dictionary of preset arguments per preset
        public List<string> argumentList = new List<string>();
        public List<string> dropsToShow = new List<string>();
        public List<CheckBox> dropsCheckboxes;

        public Dictionary<string, string> presets;
        public Dictionary<string, string> presetArguments;
        public Dictionary<string, string> alternativeLocationNames;

        public Dictionary<string, Dictionary<string, int>> locationDictionaries = new Dictionary<string, Dictionary<string, int>>();

        private BackgroundWorker bgWorker;

        //soul of bat - long library (behind mist gate)
        //fire of bat - clock tower (top right corner of large open space)
        //echo of bat - orlox's quarters, beat orlox
        //force of echo - reverse caverns
        //soul of wolf - outer wall (elevator)
        //power of wolf - entrance, starting room
        //skill of wolf - alchemy lab
        //form of mist - colosseum (after mino/werewolf fight)
        //power of mist - castle keep (above leap stone, right below richter/shaft fight)
        //gas cloud - floating catacombs
        //cube of zoe - entrance (right after death)
        //spirit orb - marble gallery
        //gravity boots - marble gallery
        //leap stone - castle keep (flea riders, lower check)
        //holy symbol - underground caverns (requires merman statue)
        //faerie scroll - long library (3 gray books, top right)
        //jewel of open - shop
        //merman statue - underground caverns
        //bat card - alchemy lab
        //ghost card - castle keep
        //faerie card - long library
        //demon card - abandoned mine
        //sword card - orlox's quarters, breakable ceiling below orlox room
        //sprite card - orlox's quarters (not available, placeholder)
        //nosedevil card - colosseum (not available, placeholder)
        //heart of vlad - anti-chapel (medusa)
        //tooth of vlad - reverse outer wall (creature)
        //rib of vlad - death wing's lair (reverse orlox, akmodan ii)
        //ring of vlad - reverse clock tower (darkwing bat)
        //eye of vlad - cave (death)
        //spike breaker - catacombs (right side, after dark spike room)
        //gold ring - succubus
        //silver ring - bell tower (spiked passage -> mist gate -> jewel of open -> maria)
        //holy glasses - clock room (requires gold and silver ring)


        public Form1()
        {
            InitializeComponent();

            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler(BGWorker_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGWorker_RunWorkerCompleted);

            startNumeric.Maximum = decimal.MaxValue;
            stopNumeric.Maximum = decimal.MaxValue;
            randomizerDirectoryTextbox.Text = Properties.Settings.Default.randomizerDirectory;

            foreach (string loc in lycanthropeLocations)
            {
                locationDictionaries.Add(loc, new Dictionary<string, int>());
                foreach (string loc2 in lycanthropeLocations)
                {
                    locationDictionaries[loc].Add(loc2, 0);
                }
            }

            alternativeLocationNames = new Dictionary<string, string>
            {
                { "Soul of Bat", "Long Library - Mist Gate" },
                { "Fire of Bat", "Clock Tower" },
                { "Echo of Bat", "Orlox" },
                { "Force of Echo", "Reverse Holy Symbol" },
                { "Soul of Wolf", "Elevator" },
                { "Power of Wolf", "Entrance - Alucard Start" },
                { "Skill of Wolf", "Alchemy Lab - Turkey Lags" },
                { "Form of Mist", "Werewolf and Minotaurus" },
                { "Power of Mist", "Castle Keep - Flea Ladder" },
                { "Gas Cloud", "Galamoth" },
                { "Cube of Zoe", "First Relic" },
                { "Spirit Orb", "Marble Gallery - Marionettes" },
                { "Gravity Boots", "Above Clock Room" },
                { "Leap Stone", "Castle Keep - Bottom of Flea Ladder" },
                { "Holy Symbol", "Underground Caverns - Requires Merman Statue" },
                { "Faerie Scroll", "Long Library - Top Right" },
                { "Jewel of Open", "Shop" },
                { "Merman Statue", "Underground Caverns - Waterfall Route" },
                { "Bat Card", "Alchemy Lab - Gaibon and Slogra" },
                { "Ghost Card", "Castle Keep - Above Broken Stairs" },
                { "Faerie Card", "Long Library - Top Left" },
                { "Demon Card", "Abandoned Mine - Left of Portal" },
                { "Sword Card", "Orlox's Quarters - Breakable Ceiling" },
                { "Heart of Vlad", "Medusa" },
                { "Tooth of Vlad", "Creature" },
                { "Rib of Vlad", "Reverse Orlox - Akmodan II" },
                { "Ring of Vlad", "Darkwing Bat" },
                { "Eye of Vlad", "Death" },
                { "Spike Breaker", "After Catacombs Spike Room" },
                { "Gold ring", "Succubus" },
                { "Silver ring", "Bell Tower - Spiked Passage" },
                { "Holy glasses", "Room of Ring Requirement" },
                { "Trio", "Trio" },
                { "Ring of Arcana", "Beelzebub" },
                { "Mormegil", "Granfaloon" },
                { "Crystal cloak", "Scylla" },
                { "Dark Blade", "Reverse Scylla" },
                { "Sprite Card", "Placeholder" },
                { "Nosedevil Card", "Placeholder" }
            };

            presets = new Dictionary<string, string>
            {
                { "Adventure", "adventure.json" },
                { "Bat-Master", "bat-master.json" },
                { "Casual", "casual.json" },
                { "Empty Handed", "empty-handed.json" },
                { "Expedition", "expedition.json" },
                { "Gem Farmer", "gem-farmer.json" },
                { "Glitch", "glitch.json" },
                { "Guarded OG", "guarded.json" },
                { "Lycanthrope", "lycanthrope.json" },
                { "Nimble", "nimble.json" },
                { "OG", "og.json" },
                { "Rat Race", "rat-race.json" },
                { "Safe", "safe.json" },
                { "Scavenger", "scavenger.json" },
                { "Speedrun", "speedrun.json" },
                { "Third Castle", "third-castle.json" },
                { "Warlock", "warlock.json" },
            };

            dropsCheckboxes = new List<CheckBox>
            {
                dropsGoldRingCheckbox,
                dropsSilverRingCheckbox,
                dropsHolyGlassesCheckbox,
                dropsGravityBootsCheckbox,
                dropsLeapStoneCheckbox,
                dropsJewelOfOpenCheckbox,
                dropsSpikeBreakerCheckbox,
                dropsMermanStatueCheckbox,
                dropsRingOfVladCheckbox,
                dropsHeartOfVladCheckbox,
                dropsRibOfVladCheckbox,
                dropsToothOfVladCheckbox,
                dropsEyeOfVladCheckbox,
                dropsHolySymbolCheckbox,
                dropsCubeOfZoeCheckbox,
                dropsSpiritOrbCheckbox,
                dropsFaerieScrollCheckbox,
                dropsSoulOfBatCheckbox,
                dropsFireOfBatCheckbox,
                dropsEchoOfBatCheckbox,
                dropsForceOfEchoCheckbox,
                dropsFormOfMistCheckbox,
                dropsPowerOfMistCheckbox,
                dropsGasCloudCheckbox,
                dropsSoulOfWolfCheckbox,
                dropsPowerOfWolfCheckbox,
                dropsSkillOfWolfCheckbox,
                dropsBatCardCheckbox,
                dropsGhostCardCheckbox,
                dropsFaerieCardCheckbox,
                dropsDemonCardCheckbox,
                dropsSwordCardCheckbox
            };

            foreach(CheckBox chk in dropsCheckboxes)
            {
                chk.CheckedChanged += SetDropsToShow;
            }

            presetsCombobox.Items.Clear();
            //foreach(KeyValuePair<string, string> kvp in presets)
            //{
            //    presetsCombobox.Items.Add(kvp.Key);
            //}
            presetsCombobox.Items.Add("Lycanthrope");
            presetsCombobox.SelectedItem = "Lycanthrope";

            ToolTip tt1 = new ToolTip();
            tt1.AutoPopDelay = 20000;
            tt1.InitialDelay = 500;
            tt1.ReshowDelay = 400;
            tt1.ShowAlways = true;
            tt1.SetToolTip(addRunBtn, "Add data to the currently shown run.");
            tt1.SetToolTip(newRunBtn, "Wipe current run and show only new data.");
            tt1.SetToolTip(stopBtn, "Stop after the currently running seed.");
            tt1.SetToolTip(showAlternateNamesCheckbox, "Show alternate names for relics and locations. Does not affect export.");
            tt1.SetToolTip(clearTextBtn, "Clears the result text. Does not get rid of the data from the last run.");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void SetDropsToShow(object sender, EventArgs e)
        {
            dropsToShow.Clear();

            foreach(CheckBox chk in dropsCheckboxes)
            {
                if (chk.Checked) dropsToShow.Add(chk.Tag.ToString());
            }

            if (resultText != string.Empty) UpdateResultsText();
        }

        public void SetArgumentString()
        {
            args = "/C node randomize -vvv -f presets/lycanthrope.json -s ";
            argumentList.Clear();
            argumentList.Add("/C node randomize -vvv -f ");
            argumentList.Add("presets/" + presets[presetsCombobox.Text] + " ");
            argumentList.Add("-s ");
            args = "";
            foreach(string arg in argumentList)
            {
                args += arg;
            }
            //MessageBox.Show(args);
        }

        private void addRunBtn_Click(object sender, EventArgs e)
        {
            if (presetsCombobox.SelectedIndex == -1)
            {
                MessageBox.Show("A preset must be selected!");
                return;
            }
            SetArgumentString();

            startSeed = (int)startNumeric.Value;
            stopSeed = (int)stopNumeric.Value;
            currentSeed = startSeed;

            continueExecuting = true;
            startTime = DateTime.Now;
            statusLabel.Text = "Processing " + (stopSeed - startSeed + 1) + " seeds...";
            bgWorker.RunWorkerAsync();
            newRunBtn.Enabled = false;
            addRunBtn.Enabled = false;
        }

        private void newRunBtn_Click(object sender, EventArgs e)
        {
            if (presetsCombobox.SelectedIndex == -1)
            {
                MessageBox.Show("A preset must be selected!");
                return;
            }
            SetArgumentString();

            totalSeeds = 0;
            locationDictionaries.Clear();
            foreach (string loc in lycanthropeLocations)
            {
                locationDictionaries.Add(loc, new Dictionary<string, int>());
                foreach (string loc2 in lycanthropeLocations)
                {
                    locationDictionaries[loc].Add(loc2, 0);
                }
            }

            startSeed = (int)startNumeric.Value;
            stopSeed = (int)stopNumeric.Value;
            currentSeed = startSeed;

            continueExecuting = true;
            startTime = DateTime.Now;
            statusLabel.Text = "Processing " + (stopSeed - startSeed + 1) + " seeds...";
            bgWorker.RunWorkerAsync();
            newRunBtn.Enabled = false;
            addRunBtn.Enabled = false;
        }

        private void BGWorker_DoWork(object sender, EventArgs e)
        {
            while (continueExecuting) //or more seeds to go
            {
                MethodInvoker inv = delegate {currentSeedLabel.Text = "Running Seed " + currentSeed + "..."; };
                Invoke(inv);

                ProcessStartInfo startInfo = new ProcessStartInfo("CMD.exe");
                //startInfo.WorkingDirectory = @"C:\Users\Trogdor\SotN-Randomizer";
                startInfo.WorkingDirectory = randomizerDirectoryTextbox.Text;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardError = true;
                startInfo.Arguments = args + currentSeed;
                startInfo.CreateNoWindow = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();
                //string txt = "";
                while (!process.HasExited)
                {
                    resultText += process.StandardOutput.ReadToEnd();
                }

                for(int n = 0; n < 3; n++)
                {
                    int i = resultText.IndexOf(":");
                    if(i >= 0) resultText = resultText.Substring(i + 1);
                }

                resultText = resultText.Substring(1);
                
                string[] rands = resultText.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                
                resultText = "";
                for(int i = 0; i < rands.Length; i++)
                {
                    rands[i] = rands[i].Trim();
                    //txt += rands[i] + "\r\n";
                    string[] row = rands[i].Split(new string[] { " at " }, StringSplitOptions.None);
                    if(row.Length > 1)
                    {
                        //txt += row[0] + " - " + row[1] + "\r\n";
                        string relic = row[0];
                        string loc = row[1];

                        locationDictionaries[loc][relic] += 1;
                    }
                }

                MethodInvoker inv2 = delegate
                {
                    TimeSpan totalTime = DateTime.Now - startTime;
                    timeElapsedLabel.Text = "Time Elapsed: " + totalTime.Hours.ToString("00") + ":" + totalTime.Minutes.ToString("00") + ":" + totalTime.Seconds.ToString("00");
                };
                Invoke(inv2);

                currentSeed++;

                if (currentSeed > stopSeed)
                {
                    currentSeed--;
                    continueExecuting = false;
                }
            }
        }

        private void BGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            stopTime = DateTime.Now;
            statusLabel.Text = "Stopped at Seed " + (currentSeed);
            totalSeeds += (currentSeed - startSeed) + 1;
            currentSeedLabel.Text = "Done. Total seeds for calculation: " + totalSeeds;
            startNumeric.Value = currentSeed + 1;
            //resultsRtb.Text = txt;
            newRunBtn.Enabled = true;
            addRunBtn.Enabled = true;
            TimeSpan totalTime = stopTime - startTime;
            timeElapsedLabel.Text = "Time Elapsed: " + totalTime.Hours.ToString("00") + ":" + totalTime.Minutes.ToString("00") + ":" + totalTime.Seconds.ToString("00");

            UpdateResultsText();

            //for saving, have a seed range and the complete results
        }

        public void UpdateResultsText()
        {
            Dictionary<string, int> dropsPerLocation = new Dictionary<string, int>();
            foreach(string loc in lycanthropeLocations)
            {
                dropsPerLocation.Add(loc, 0);
            }

            int totalTrackedDrops = 0;
            foreach(KeyValuePair<string, Dictionary<string, int>> loc in locationDictionaries)
            {
                foreach(string drop in dropsToShow)
                {
                    totalTrackedDrops += loc.Value[drop];
                    dropsPerLocation[loc.Key] += loc.Value[drop];
                }
            }
            //MessageBox.Show("TD: " + totalTrackedDrops);

            resultText = "";
            foreach (KeyValuePair<string, Dictionary<string, int>> loc in locationDictionaries)
            {
                resultText += loc.Key; // + " (" + alternativeLocationNames[loc.Key] + "):\r\n";
                if (showAlternateNamesCheckbox.Checked)
                    resultText += " (" + alternativeLocationNames[loc.Key] + ") ";
                
                resultText += ": " + ((dropsPerLocation[loc.Key] / (float)totalSeeds) * 100).ToString("0.00") + "%\r\n";

                foreach (string drop in dropsToShow)
                {
                    if (loc.Value[drop] > 0)
                    {
                        resultText += " " + drop + ": " + loc.Value[drop] + "\r\n";
                    }
                }
                resultText += "\r\n";
            }
            resultsRtb.Text = resultText;
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {
            continueExecuting = false;
        }

        private void startNumeric_ValueChanged(object sender, EventArgs e)
        {
            if((int)startNumeric.Value > (int)stopNumeric.Value)
            {
                stopNumeric.Value = startNumeric.Value;
            }
        }

        private void showAlternateNamesCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (resultText != string.Empty) UpdateResultsText();
        }

        private void clearTextBtn_Click(object sender, EventArgs e)
        {
            resultsRtb.Clear();
        }

        private void exportRunBtn_Click(object sender, EventArgs e)
        {
            //save to xml or whatever
        }

        private void directoryGetBtn_Click(object sender, EventArgs e)
        {
            randomizerDirectoryTextbox.Text = Properties.Settings.Default.randomizerDirectory;
        }

        private void directorySetBtn_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.randomizerDirectory = randomizerDirectoryTextbox.Text;
            Properties.Settings.Default.Save();
        }





        /*
         while (continueExecuting) //or more seeds to go
            {
                //statusLabel.Text = "Running Seed " + currentSeed + "...";


                ProcessStartInfo startInfo = new ProcessStartInfo("CMD.exe");
                startInfo.WorkingDirectory = @"C:\Users\Trogdor\SotN-Randomizer";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardError = true;
                startInfo.Arguments = args + currentSeed;
                startInfo.CreateNoWindow = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();
                
                while (!process.HasExited)
                {
                    txt += process.StandardOutput.ReadToEnd();
                }

                resultsRtb.Text = txt;


                currentSeed++;

                if (currentSeed > stopSeed)
                {
                    continueExecuting = false;

                }

                //continueExecuting = false; //remove eventually

            }
         */
    }
}
