using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;
using SlipegFramework;

namespace DevChrono
{
    sealed class Chrono
    {
        private Stopwatch Watch = new Stopwatch();
        private Stopwatch BeepTimer = new Stopwatch();
        private Thread WaitingInput;
        private Process KeyStrokeCounter = new Process();
        private ConsoleUtillity Cu = new ConsoleUtillity();
        private SlipegFramework.Debug _Debug = new SlipegFramework.Debug();

        private bool IsDead = false;
        private bool PositionMemory = false;
        private bool TheresNote = false;
        private bool FirstTime = false;
        private bool ToRemove = false;

        private string IsDone = "";
        private string MyObjectif = "";
        private string ProjectName = "";
        private string ProjectVersion = "1.0";
        private string MyTechUsed = "";

        private char WorkingCharacter;
        private string[] PreferencesNames = {"Ask for version:","Use last project name instead of asking:","Ask to specified the tech used:"};
        private readonly bool[] PreferencesBool = { Properties.Settings.Default.AskForVersion, Properties.Settings.Default.AskForProjectName,Properties.Settings.Default.AskForTech};
        private readonly char[] ForbiddenChars = new char[] {'[',']','{','}'};

        private int Strokes = 0;
        private int TotalStrokes = 0;
        private int TotalHours = 0;

        private List<string> Changes = new List<string>();
        private List<string> KnownBug = new List<string>();

        private readonly string[] Signature = { @"  .  -  _ _     ..   .    ChronoDev-Version-2.0",      
                                      @"-  ----| (_)------ ----------- ---------+*      -   .     .   ",
                                      @"-- -___| |_ _ __ . ___  __ _ --- -+*          .",
                                      @"- -/ __| | | '_ \ / _ \/ _` |---- -------- ----*   - .",
                                      @"---\__ \ | | |_) |  __/ (_| |----- -+*  .",
                                      @"- -|___/_|_| .__/ \___|\__, |---+*         -  .",
                                      @"--   -   --| |--------- __/ |---------- -+*        .        .",
                                      @"- --- -----|_|- -- ----|___/ ------ ------- --------+*",
                                      @".   -    _          -       .     ,  ",
                                    };
        //Get current time for later output
        private readonly string CurrentTime = DateTime.Now.ToString();
        private string Note = "";
        public Chrono()
        {
            Console.SetWindowSize(111, 37);
            Console.Beep();
            if (!File.Exists(Environment.CurrentDirectory + "\\tths.bin"))
            {
                File.Create(Environment.CurrentDirectory + "\\tths.bin");
            }
            if (!File.Exists(Environment.CurrentDirectory + "\\ttsk.bin"))
            {
                File.Create(Environment.CurrentDirectory + "\\ttsk.bin");
            }
            try
            {
                TotalHours = GetTotalHours();
                TotalStrokes = GetTotalStrokes();
            }
            catch(Exception e)
            {
                FirstTime = true;
                TotalHours = 0;
                TotalStrokes = 0;
            }
            
            Changes.Add("Preferences menu.");
            Changes.Add("Keystrokes counter.");
            Changes.Add("Abort exit.");
            Changes.Add("Total hours counter.");
            Changes.Add("Total Strokes counter.");
            Changes.Add("Other changes have been made but not interesting to show you.");
            KnownBug.Add("Crash at first startup goes well after.");
            GeneralUtillity.CreateChangelog(Changes,KnownBug, "ChronoDev", "2.0",DateTime.Now.ToString());
            

            WelcomeMessages();
            MainMenu();
        }
        //Thread
        private void MainMenu()
        {
            while (true)
            {
                Console.WriteLine("\n\nWhat would you like to do?\nUse number to choose:\n---------------------|\n1.Start chrono\n2.Show History\n3.Preferences\n4.Exit");
                var Answer = Console.ReadKey();
                if (Answer.Key == ConsoleKey.D1)
                {
                    NormalChrono();
                }
                else if (Answer.Key == ConsoleKey.D2)
                {
                    ShowHistory();
                }
                else if (Answer.Key == ConsoleKey.D3)
                {
                    SettingPreferences();
                }
                else if (Answer.Key == ConsoleKey.D4)
                {
                    Environment.Exit(0);
                }
            }
        }
        private void SettingPreferences()
        {
            Console.WriteLine("\n");
            for (int i = 0; i<PreferencesNames.Length;i++)
            {
                Console.WriteLine((i+1)+". "+PreferencesNames[i]+PreferencesBool[i]);
            }
            Console.WriteLine("9. Exit to menu and save");
            ConsoleKeyInfo Answer;
            while (true)
            {
                Answer = Console.ReadKey();
                switch (Answer.Key)
                {
                    case ConsoleKey.D1:
                        if (Properties.Settings.Default.AskForVersion)
                        {
                            Properties.Settings.Default.AskForVersion = false;
                            Console.WriteLine("\nThe app will now stop asking you for a version in the future.");
                            SettingPreferences();
                        }
                        else
                        {
                            Properties.Settings.Default.AskForVersion = true;
                            SettingPreferences();
                        }
                        break;
                    case ConsoleKey.D2:
                        if (Properties.Settings.Default.AskForProjectName)
                        {
                            Properties.Settings.Default.AskForProjectName = false;
                            Console.WriteLine("\nThe app will now stop asking for Project name, instead will use your last used name.");
                            SettingPreferences();
                        }
                        else
                        {
                            Properties.Settings.Default.AskForProjectName = true;
                            SettingPreferences();
                        }
                        break;
                    case ConsoleKey.D3:
                        if (Properties.Settings.Default.AskForTech)
                        {
                            Properties.Settings.Default.AskForTech = false;
                            Console.WriteLine("\nThe app will now stop asking for Project name, instead will use your last used name.");
                            SettingPreferences();
                        }
                        else
                        {
                            Properties.Settings.Default.AskForTech = true;
                            SettingPreferences();
                        }
                        break;
                    case ConsoleKey.D9:
                        Properties.Settings.Default.Save();
                        Console.WriteLine("\nYou need to reload the application to see the changes.");
                        Thread.Sleep(2000);
                        MainMenu();
                        break;
                }
            }
        }
        private void ShowHistory()
        {
            if (File.Exists(Environment.CurrentDirectory + "/TimeHistory.txt"))
            {
                Console.WriteLine("\n");
                foreach (string Lines in File.ReadLines(Environment.CurrentDirectory + "/TimeHistory.txt"))
                {
                    CheckCharColor(Lines);
                }
            }
            else
            {
                Console.WriteLine("\nHistory file doesn't exist.");
            }
        }
        public void WaitingInputThread()
        {
            //Waiting for commands
            while (true)
            {
                var tempon = Console.ReadKey(true);
                KeyStrokeCounter.Kill();
                Watch.Stop();
                if (tempon != null)
                {
                SetTotalHours();
                SetTotalStrokes();
                    IsDead = true;
                    if (tempon.Key == ConsoleKey.N)
                    {
                        while (true)
                        {
                            ConsoleKeyInfo Answer;
                            Console.WriteLine("\nWould you like to put a note ? \n (y)es/(n)o\n");
                            Answer = Console.ReadKey();
                            if (Answer.Key == ConsoleKey.N)
                            {
                                break;
                            }
                            else if (Answer.Key == ConsoleKey.Y)
                            {
                                TheresNote = true;
                                string mess = "";
                                Note = "NOTE:{ ";
                                do
                                {
                                    Console.Write("Please enter your note :");
                                    mess = Console.ReadLine();
                                } while (CheckForBadChars(mess) == true);
                                Note += mess;
                                Note += "}";
                                break;
                            }
                        }
                    }
                    else if (tempon.Key == ConsoleKey.E)
                    {
                        Environment.Exit(0);
                    }
                    while (true)
                    {
                        if (Properties.Settings.Default.AskForVersion)
                        {
                            do
                            {
                                Console.Write("\nPlease enter your project version:");
                                ProjectVersion = Console.ReadLine();

                            } while (CheckForBadChars(ProjectVersion) == true);

                            Console.WriteLine("Does this version satisfy you: {0}\n(Y)es\n(N)o", ProjectVersion);
                            var Answer = Console.ReadKey();
                            if (Answer.Key == ConsoleKey.Y)
                            {
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    //Your done project
                    while (true)
                    {
                        ConsoleKeyInfo Answer;
                        Console.WriteLine("\nIs the project done ? \n(y)es\n(n)o\n");
                        Answer = Console.ReadKey();
                        if (Answer.Key == ConsoleKey.N)
                        {
                            IsDone = " Work in progress.";
                            break;
                        }
                        else if (Answer.Key == ConsoleKey.Y)
                        {

                            IsDone = " Project is done, new things may be developped further in the future.";
                            break;
                        }
                    }
                    //write time and project into a file
                    using (StreamWriter WriteTime = new StreamWriter(Environment.CurrentDirectory + "/TimeHistory.txt", true))
                    {
                        WriteTime.WriteLine("                                               =====");
                        WriteTime.WriteLine("***********************************************=====**********************************************************");
                        WriteTime.WriteLine("-----------------------------------------------START----------------------------------------------------------");
                        Strokes = GetStrokes();

                        WriteTime.WriteLine("Coding session: [Date: {0}] Elapsed-time: [{1}H {2}m {3}s {4}ms] Total-KeyStrokes:[{5}]/////////", CurrentTime,
                            Watch.Elapsed.Hours, Watch.Elapsed.Minutes, Watch.Elapsed.Seconds, Watch.Elapsed.Milliseconds, Strokes);
                        WatchOutput(WriteTime, MyObjectif);
                        if (TheresNote == true)
                        {
                            WriteTime.WriteLine();
                            WriteTime.WriteLine(Note);
                        }
                        WriteTime.WriteLine("Technologies used: {0}", MyTechUsed);
                        WriteTime.WriteLine("State of the project: {0}", IsDone);
                        WriteTime.WriteLine("");
                        WriteTime.WriteLine("Project Name: [{0}]", ProjectName);
                        WriteTime.WriteLine("Version: [{0}]", ProjectVersion);
                        WriteTime.WriteLine("-----------------------------------------------_END_----------------------------------------------------------");
                        WriteTime.WriteLine("***********************************************=====**********************************************************");
                        WriteTime.WriteLine("                                               =====");
                    }
                    Environment.Exit(0);
                }
            }
        }
        private void WatchOutput(StreamWriter WriteTime, string MyObjectif)
        {
            //See if the objectif variable have a length inferior to 40 characters
            if (MyObjectif.ToCharArray().Length < 70)
            {
                //then just print the output normaly
                WriteTime.WriteLine("The objectif of this session is: {0}", "-{ " + MyObjectif + " }");
            }
            //if IT IS supperior to 40 chars
            else if (MyObjectif.ToCharArray().Length > 70 && MyObjectif.ToCharArray().Length < 130)
            {
                //Copy the variable to a char array
                char[] ObjectiveArray = MyObjectif.ToCharArray();
                

                //init two paragraphs
                string paragraph1 = "";
                string paragraph2 = "";

                //going through each characters
                for (int i = 0; i <= ObjectiveArray.Length - 1; i++)
                {
                    //if inferior or equals to 40 then add each characters to paragraph 1
                    if (i <= 70)
                    {
                        paragraph1 += ObjectiveArray[i];
                    }
                    //if supperior the rest is added to the seconde paragraph
                    else
                    {
                        paragraph2 += ObjectiveArray[i];
                    }
                }

                //then print the data in a file on two line
                WriteTime.WriteLine("SESSION OBJECTIF:");
                WriteTime.WriteLine("-{ " + paragraph1 + " }");
                WriteTime.WriteLine("-{ " + paragraph2 + " }");
            }
            //PARAGRAPHE 3
            else if (MyObjectif.ToCharArray().Length > 130 && MyObjectif.ToCharArray().Length < 160)
            {
                //Copy the variable to a char array
                char[] ObjectiveArray = MyObjectif.ToCharArray();
                //init two paragraphs
                string paragraph1 = "";
                string paragraph2 = "";
                string paragraph3 = "";

                //going through each characters
                for (int i = 0; i <= ObjectiveArray.Length - 1; i++)
                {
                    //if inferior or equals to 40 then add each characters to paragraph 1
                    if (i <= 70)
                    {
                        paragraph1 += ObjectiveArray[i];
                    }
                    //if supperior the rest is added to the seconde paragraph
                    else if (i > 70 && i < 130)
                    {
                        paragraph2 += ObjectiveArray[i];
                    }
                    else if (i>130)
                    {
                        paragraph3 += ObjectiveArray[i];
                    }
                }

                //then print the data in a file on two line
                WriteTime.WriteLine("SESSION OBJECTIF:");
                WriteTime.WriteLine("-{ " + paragraph1 + " }");
                WriteTime.WriteLine("-{ " + paragraph2 + " }");
                WriteTime.WriteLine("-{ " + paragraph3 + " }");
            }
            else if (MyObjectif.ToCharArray().Length > 170)
            {
                //Copy the variable to a char array
                char[] ObjectiveArray = MyObjectif.ToCharArray();

                //init two paragraphs
                string paragraph1 = "";
                string paragraph2 = "";
                string paragraph3 = "";
                string paragraph4 = "";

                //going through each characters
                for (int i = 0; i <= ObjectiveArray.Length - 1; i++)
                {
                    //if inferior or equals to 40 then add each characters to paragraph 1
                    if (i <= 70)
                    {
                        paragraph1 += ObjectiveArray[i];
                    }
                    //if supperior the rest is added to the seconde paragraph
                    else if (i > 70 && i < 140)
                    {
                        paragraph2 += ObjectiveArray[i];
                    }
                    else if (i > 140 && i < 210)
                    {
                        paragraph3 += ObjectiveArray[i];
                    }
                    else if (i > 210)
                    {
                        paragraph4 += ObjectiveArray[i];
                    }
                }

                //then print the data in a file on two line
                WriteTime.WriteLine("SESSION OBJECTIF:");
                WriteTime.WriteLine("-{ " + paragraph1 + " }");
                WriteTime.WriteLine("-{ " + paragraph2 + " }");
                WriteTime.WriteLine("-{ " + paragraph3 + " }");
                WriteTime.WriteLine("-{ " + paragraph4 + " }");
            }
        }
        private void WelcomeMessages()
        {
            foreach (string line in Signature)
            {
                Console.ForegroundColor = Cu.ChoseRandomColor();
                Console.WriteLine(line);
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("+------------------------------------+");
            Console.WriteLine("| Note:                              |\n| To save the time in a file,        | \n| press any key then it's gonna save |\n| a file in the same directory       |\n| that the programme with            |\n| the time as output.                |");
            Console.Write("|");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(" Closing the window will");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("            |\n");
            Console.Write("|");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(" result in a loss of the data.");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("      |\n");
            Console.WriteLine("+------------------------------------+");
            Console.WriteLine("\nHere is some hotkey(s) to remember->\n- To add a note press 'N'.\n- To abort and exit press 'E'. ");
            Console.WriteLine("\nGlobal stats:\n- Total-hours:[{0}]\n- Total-strokes:[{1}]",TotalHours,TotalStrokes);
        }
        private void NormalChrono()
        {
            if (Properties.Settings.Default.AskForProjectName == false)
            {
                do
                {
                    Console.Write("\nProject name:");
                    ProjectName = Console.ReadLine();
                    Properties.Settings.Default.LastProjectName = ProjectName;
                    Properties.Settings.Default.Save();

                } while (CheckForBadChars(ProjectName) == true);
            }
            else
            {
                if (Properties.Settings.Default.LastProjectName!="")
                {
                    ProjectName = Properties.Settings.Default.LastProjectName;
                }
                else
                {
                    ProjectName = "NoName";
                }
            }
            do
            {
                Console.Write("\nObjective of this project/session:");
                MyObjectif = Console.ReadLine();

            } while (CheckForBadChars(MyObjectif) == true);

            if (Properties.Settings.Default.AskForTech)
            {
                do
                {
                    Console.Write("\nPlease specify what tech you are using:");
                    MyTechUsed = Console.ReadLine();
                } while (CheckForBadChars(MyTechUsed) == true);
            }
            else
            {
                MyTechUsed = "Not specified.";
            }
            //If ProjectName is not set then i set NoName as its default value
                if (ProjectName == "")
                    ProjectName = "NoName";

            //Making the thread
            WaitingInput = new Thread(new ThreadStart(WaitingInputThread));


            Watch.Start();
            BeepTimer.Start();
            
            KeyStrokeCounter.StartInfo.FileName="KeyStrokeCounter.exe";
            KeyStrokeCounter.StartInfo.Arguments = "true";
            KeyStrokeCounter.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            KeyStrokeCounter.Start();
            while (true)
            {
                
                var WatchT = Watch.Elapsed;
                //Verify that the thread is alive
                if (!WaitingInput.IsAlive)
                {
                    //Working in background
                    WaitingInput.IsBackground = true;
                    //Starting the thread
                    WaitingInput.Name = "WaitingInput";
                    WaitingInput.Start();
                }
                BeepConsole();
                if (IsDead == false)
                {
                    Console.WriteLine("Elapsed time: {0}H {1}m {2}s {3}ms ", WatchT.Hours, WatchT.Minutes, WatchT.Seconds, WatchT.Milliseconds);
                    Thread.Sleep(1);
                }
            }
        }
        
        private void CheckCharColor(string Line)
        {
            char[] Characters = Line.ToCharArray();
            for (int i = 0; i<= Characters.Length-1; i++)
            {
                if (Characters[i].Equals('*'))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write('*');
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (Characters[i].Equals('='))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write('=');
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (Characters[i].Equals('-'))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write('-');
                    Console.ForegroundColor = ConsoleColor.White;
                }

                //Verify spaces
                else if (Characters[i].Equals(' '))
                {
                    Console.Write(' ');
                }
                else if (Characters[i].Equals('[') || PositionMemory == true && WorkingCharacter == '[')
                {
                   TinyParse(Characters[i], '[',']',ConsoleColor.Green,false);
                }
                else if (Characters[i].Equals('{') || PositionMemory == true && WorkingCharacter == '{')
                {
                   TinyParse(Characters[i], '{', '}', ConsoleColor.Yellow,true);
                }
                else
                {
                    Console.Write(Characters[i]);
                }
            }
            Console.WriteLine();
          //  PositionMemory = false;
        }
        private void TinyParse(char ValueToCheck, char Bracket1, char Bracket2, ConsoleColor Color, bool ToRem)
        {
            ToRemove = ToRem;
            WorkingCharacter = Bracket1;
            if (ValueToCheck.Equals(Bracket1) || PositionMemory == true)
            {
                PositionMemory = true;
                if (!ValueToCheck.Equals(Bracket1))
                {
                    if (ValueToCheck.Equals(Bracket2))
                    {
                        PositionMemory = false;
                        if(ToRemove == false)
                        Console.Write(Bracket2);
                    }
                    else if (PositionMemory == true)
                    {
                        Console.ForegroundColor = Color;
                        Console.Write(ValueToCheck);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else if (ValueToCheck.Equals(Bracket1))
                {   
                    if(ToRemove == false)
                    Console.Write(Bracket1);
                }
            }
        }
        private int GetStrokes()
        {
            int count = 0;
            count = (int)SlipegFramework.IO.ReadToBinary(Environment.CurrentDirectory, "Strks.bin", IO.TypeValue.IntValue);

            return count;
        }
        private int GetTotalHours()
        {
            int FileHours;
            FileHours = (int)SlipegFramework.IO.ReadToBinary(Environment.CurrentDirectory, "tths.bin", IO.TypeValue.IntValue);
            return FileHours;
        }
        private void SetTotalHours()
        {
            int Hours = 0;
            int FileHours = 0;

            Hours = Watch.Elapsed.Hours;
            if (FirstTime == false)
            {
                FileHours = (int)SlipegFramework.IO.ReadToBinary(Environment.CurrentDirectory, "tths.bin", IO.TypeValue.IntValue);
            }
            FileHours += Hours;
            SlipegFramework.IO.WriteToBinary(Environment.CurrentDirectory,"tths.bin",FileHours);
        }
        private void SetTotalStrokes()
        {
            int strks = 0;
            int FileStrks = 0;

            strks = GetStrokes();
            if (FirstTime == false)
            {
                FileStrks = (int)SlipegFramework.IO.ReadToBinary(Environment.CurrentDirectory, "ttsk.bin", IO.TypeValue.IntValue);
            }
            FileStrks += strks;
            SlipegFramework.IO.WriteToBinary(Environment.CurrentDirectory, "ttsk.bin", FileStrks);
        }
        private int GetTotalStrokes()
        {
            int ttstrokes = 0;
            ttstrokes = (int)SlipegFramework.IO.ReadToBinary(Environment.CurrentDirectory, "ttsk.bin", IO.TypeValue.IntValue);
            return ttstrokes;
        }
        
        private void BeepConsole()
        {
            if (BeepTimer.Elapsed.Minutes > 10)
            {
                Console.Beep();
                BeepTimer.Reset();
                BeepTimer.Start();
            }
        }
        private bool CheckForBadChars(string Line)
        {
            char[] ArrLine = Line.ToCharArray();
            int ArrLineLength = ArrLine.Length;
            bool IsForbidden = false;

            for (int a = 0; a < ArrLineLength; a++)
            {
                for (int b = 0; b < ForbiddenChars.Length; b++)
                {
                    if (ArrLine[a] == ForbiddenChars[b])
                    {
                        IsForbidden = true;
                        return IsForbidden;
                    }
                }
            }
            return IsForbidden;
        }
    }
}
