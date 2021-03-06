﻿using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace Synergy_Solutions_App
{
    public partial class StartScreen : Form
    {
        //initalize random seed(used for testing without a MBED)
        Random rnd = new Random();
        public int GAMESPLAYED = 0;
        public int HIGHSCORE = 0;
        //all displayed text with a lanuage select varible
        public static int lanSelect = 0;
        public static int theScore;
        int numInstructions = 5;
        int loadingTick = 0;

        //getting size of the screen (changes depending on screen used)
        public static int getScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
        public static int getScreenHight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;

        System.Media.SoundPlayer gameAudio = new System.Media.SoundPlayer(Properties.Resources.Castlevania);
        System.Media.SoundPlayer gameEnd = new System.Media.SoundPlayer(Properties.Resources.endSound);

        double getScreenHightInPixels = Screen.PrimaryScreen.Bounds.Height;
        double getScreenWidthInPixels = Screen.PrimaryScreen.Bounds.Width;

        //fade for the image
        public byte alpha = 255;
        bool inOut = true;

        //thread to move into UI mode
        Thread thUI;
        Thread thSerial;

        //changes the alpha value of each pixel in a screen 
        private static Bitmap changeTransparacy(Image image, Byte alpha)
        {
            Bitmap inputImage = new Bitmap(image);
            Bitmap outputImage = new Bitmap(image.Width, image.Height);
            System.Drawing.Color orignalPixel = System.Drawing.Color.Black;
            System.Drawing.Color newPixel = System.Drawing.Color.Black;


            for (int w = 0; w < image.Width; w++)
            {
                for (int h = 0; h < image.Height; h++)
                {
                    orignalPixel = inputImage.GetPixel(w, h);
                    //if the pixel is transparent do not change  (changing pixel will trun the transparant pixel black)
                    if (orignalPixel == Color.FromArgb(0, orignalPixel.R, orignalPixel.B, orignalPixel.G))
                    {
                        continue;
                    }
                    else
                    {
                        newPixel = Color.FromArgb(alpha, orignalPixel.R, orignalPixel.B, orignalPixel.G);

                        outputImage.SetPixel(w, h, newPixel);
                    }
                }

            }


            return outputImage;
        }

        //getting serial ports (used from Bernard's code, modified to auto connect if a port exists)
        public void getSerialPorts()
        {
            string[] ports;
            ports = SerialPort.GetPortNames();



            try
            {
                gameSerial.PortName = ports[0];
                gameSerial.Open();
                gameSerial.DtrEnable = true;
                Console.WriteLine("connected to:" + gameSerial.PortName + Environment.NewLine);
            }
            catch
            {
                Console.WriteLine("can't connect to serial bus" + Environment.NewLine);
                System.Windows.Forms.MessageBox.Show("Cannot connect to system please contact park staff");
            }

        }

        //write request (adapted from bernard's code) ensure's all instructions are the same format 
        private void writeRequest(string key, int no)
        {
            try
            {
                string mesg = "#" + key + ":" + no + ";";
                gameSerial.Write(mesg);

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: :" + e.Message);
            }

        }

        //get the score from serial communication if an error occurs return 99 (used for datalogging)
        public int getScore()
        {
            string score = " ";

            if (gameSerial.IsOpen)
            {
                writeRequest("gs", 1);
                score = gameSerial.ReadTo(";");

            }

            // if an error with the serial communication occurs or the serial communication is not present set the score to 99
            if (score == " ")
            {
                score = "99";
            }
            int s = Int32.Parse(score);
            return s;
        }

        public StartScreen()
        {
            InitializeComponent();

        }

        private void StartScreen_Load(object sender, EventArgs e)
        {
            //Warning message for fellow devs commented out as development is finshed
            //System.Windows.Forms.MessageBox.Show("The audio is pretty loud you may want to trun your speakers down");

            //set UI to screen size and put it in top corner of screen and to size of the screen
            this.ClientSize = new System.Drawing.Size(getScreenWidth, getScreenHight);
            this.Location = new Point(0, 0);

            //update UI
            manualUIUpdate();

           gameAudio.PlayLooping();


        }

        //----------Starts manually moving UI elements (called in form_load)---

        //Center x-y positions of a element (next two functions)
        private int centerElementXcor(int Xcor, int elementWidth)
        {
            Xcor = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2) - (elementWidth / 2);
            return Xcor;
        }

        private int centerElementYcor(int Ycor, int elementHight)
        {
            Ycor = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2) - (elementHight / 2);
            return Ycor;
        }

        //Updating the user interfance
        public void manualUIUpdate()
        {

            //edit to *0 to change position relative to center
            //startGame - button to start game
            int startGameMoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * 0.35));
            int startGameMoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * 0));

            //img_action - an image to show user what to do (used for loading image)
            int img_actionMoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * 0));
            int img_actionMoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * 0));

            //img_arrow - the fading in/out arrow
            int img_arrowMoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * 0.28));
            int img_arrowMoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * 0));

            //img_button - the odd looking white button under the fading arrow the goes once the startGame button is pressed
            int img_UFOMoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * 0));
            int img_UFOMoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * 0));

            //instruction - used for loading/ready? labels
            int instructionMoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * 0));
            int instructionMoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * 0));

            //action - place holder should be replaced with images of the instruction
            int actionMoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * 0));
            int actionMoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * 0));

            //img_planet01_1
            int img_planet01_1MoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * 0.02));
            int img_planet01_1MoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * 0.35));

            //img_planet01_2
            int img_planet01_2MoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * 0.15));
            int img_planet01_2MoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * -0.45));

            //img_planet02_1
            int img_planet02_1MoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * 0.25));
            int img_planet02_1MoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * -0.28));

            //img_planet02_2
            int img_planet02_2MoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * -0.4)); //???
            int img_planet02_2MoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * -0.38));

            //img_planet02_3
            int img_planet02_3MoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * 0.32));
            int img_planet02_3MoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * 0.32));

            //img_planet03_1
            int img_planet03_1MoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * 0.2));
            int img_planet03_1MoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * 0.2));

            //img_planet03_2
            int img_planet03_2MoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * -0.35));
            int img_planet03_2MoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * 0.4));

            //img_planet04_1
            int img_planet04_1MoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * -0.15));
            int img_planet04_1MoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * 0.24));

            //img_planet04F_1
            int img_planet04F_1MoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * 0.02));
            int img_planet04F_1MoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * -0.32));

            //img_star01_1
            int img_star01_1MoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * -0.2));
            int img_star01_1MoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * -0.35));

            //img_star01_2
            int img_star01_2MoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * -0.3));
            int img_star01_2MoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * -0.25));

            //img_star02_1
            int img_star02_1MoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * 0.05));
            int img_star02_1MoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * 0.12));

            //img_star02_2
            int img_star02_2MoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * 0.1));
            int img_star02_2MoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * -0.2));

            //Title
            int TitleMoveY = Convert.ToInt32(Math.Floor(getScreenHightInPixels * -0.35));
            int TitleMoveX = Convert.ToInt32(Math.Floor(getScreenWidthInPixels * 0));

            ///////////////////////////////////////////////////////////////////////////////////////

            //Title
            Title.Location = new Point(
            centerElementXcor(Title.Location.X, Title.Size.Width) + TitleMoveX,
            centerElementYcor(Title.Location.Y, Title.Size.Height) + TitleMoveY);
            Title.Refresh();


            //startGame
            startGame.Location = new Point(
                centerElementXcor(startGame.Location.X, startGame.Size.Width) + startGameMoveX,
                centerElementYcor(startGame.Location.Y, startGame.Size.Height) + startGameMoveY);
            startGame.Refresh();

            //img_action
            img_action.Location = new Point(
                centerElementXcor(img_action.Location.X, img_action.Size.Width) + img_actionMoveX,
                centerElementYcor(img_action.Location.Y, img_action.Size.Height) + img_actionMoveY);
            img_action.Refresh();

            //img_arrow
            img_arrow.Location = new Point(
                centerElementXcor(img_arrow.Location.X, img_arrow.Size.Width) + img_arrowMoveX,
                centerElementYcor(img_arrow.Location.Y, img_arrow.Size.Height) + img_arrowMoveY);
            img_arrow.Refresh();

            //img_UFO
            img_UFO.Location = new Point(
                centerElementXcor(img_UFO.Location.X, img_UFO.Size.Width) + img_UFOMoveX,
                centerElementYcor(img_UFO.Location.Y, img_UFO.Size.Height) + img_UFOMoveY);
            img_UFO.Refresh();
            /*
            //instruction
            instruction.Location = new Point(
                centerElementXcor(instruction.Location.X, instruction.Size.Width) + instructionMoveX,
                centerElementYcor(instruction.Location.Y, instruction.Size.Height) + instructionMoveY);
            instruction.Refresh();

            //action
            action.Location = new Point(
                centerElementXcor(action.Location.X, action.Size.Width) + actionMoveY,
                centerElementYcor(action.Location.Y, action.Size.Height) + actionMoveX);
            action.Refresh();
            */
            //img_planet01_1
            img_planet01_1.Location = new Point(
               centerElementXcor(img_planet01_1.Location.X, img_planet01_1.Size.Width) + img_planet01_1MoveX,
               centerElementYcor(img_planet01_1.Location.Y, img_planet01_1.Size.Height) + img_planet01_1MoveY);
            img_planet01_1.Refresh();

            //img_planet01_2
            img_planet01_2.Location = new Point(
             centerElementXcor(img_planet01_2.Location.X, img_planet01_2.Size.Width) + img_planet01_2MoveX,
             centerElementYcor(img_planet01_2.Location.Y, img_planet01_2.Size.Height) + img_planet01_2MoveY);
            img_planet01_2.Refresh();

            //img_planet02_1
            img_planet02_1.Location = new Point(
            centerElementXcor(img_planet02_1.Location.X, img_planet02_1.Size.Width) + img_planet02_1MoveX,
            centerElementYcor(img_planet02_1.Location.Y, img_planet02_1.Size.Height) + img_planet02_1MoveY);
            img_planet02_1.Refresh();

            //img_planet02_2
            img_planet02_2.Location = new Point(
            centerElementXcor(img_planet02_2.Location.X, img_planet02_2.Size.Width) + img_planet02_2MoveX,
            centerElementYcor(img_planet02_2.Location.Y, img_planet02_2.Size.Height) + img_planet02_2MoveY);
            img_planet02_2.Refresh();

            //img_planet02_3
            img_planet02_3.Location = new Point(
            centerElementXcor(img_planet02_3.Location.X, img_planet02_3.Size.Width) + img_planet02_3MoveX,
            centerElementYcor(img_planet02_3.Location.Y, img_planet02_3.Size.Height) + img_planet02_3MoveY);
            img_planet02_3.Refresh();

            //img_planet03_1
            img_planet03_1.Location = new Point(
            centerElementXcor(img_planet03_1.Location.X, img_planet03_1.Size.Width) + img_planet03_1MoveX,
            centerElementYcor(img_planet03_1.Location.Y, img_planet03_1.Size.Height) + img_planet03_1MoveY);
            img_planet03_1.Refresh();

            //img_planet03_2
            img_planet03_2.Location = new Point(
            centerElementXcor(img_planet03_2.Location.X, img_planet03_2.Size.Width) + img_planet03_2MoveX,
            centerElementYcor(img_planet03_2.Location.Y, img_planet03_2.Size.Height) + img_planet03_2MoveY);
            img_planet03_2.Refresh();

            //img_planet04_1
            img_planet04_1.Location = new Point(
            centerElementXcor(img_planet04_1.Location.X, img_planet04_1.Size.Width) + img_planet04_1MoveX,
            centerElementYcor(img_planet04_1.Location.Y, img_planet04_1.Size.Height) + img_planet04_1MoveY);
            img_planet04_1.Refresh();

            //img_planet04F_1
            img_planet04F_1.Location = new Point(
            centerElementXcor(img_planet04F_1.Location.X, img_planet04F_1.Size.Width) + img_planet04F_1MoveX,
            centerElementYcor(img_planet04F_1.Location.Y, img_planet04F_1.Size.Height) + img_planet04F_1MoveY);
            img_planet04F_1.Refresh();

            //img_star01_1
            img_star01_1.Location = new Point(
            centerElementXcor(img_star01_1.Location.X, img_star01_1.Size.Width) + img_star01_1MoveX,
            centerElementYcor(img_star01_1.Location.Y, img_star01_1.Size.Height) + img_star01_1MoveY);
            img_star01_1.Refresh();

            //img_star01_2
            img_star01_2.Location = new Point(
            centerElementXcor(img_star01_2.Location.X, img_star01_2.Size.Width) + img_star01_2MoveX,
            centerElementYcor(img_star01_2.Location.Y, img_star01_2.Size.Height) + img_star01_2MoveY);
            img_star01_2.Refresh();

            //img_star02_1
            img_star02_1.Location = new Point(
            centerElementXcor(img_star02_1.Location.X, img_star02_1.Size.Width) + img_star02_1MoveX,
            centerElementYcor(img_star02_1.Location.Y, img_star02_1.Size.Height) + img_star02_1MoveY);
            img_star02_1.Refresh();


            //img_star02_2
            img_star02_2.Location = new Point(
            centerElementXcor(img_star02_2.Location.X, img_star02_2.Size.Width) + img_star02_2MoveX,
            centerElementYcor(img_star02_2.Location.Y, img_star02_2.Size.Height) + img_star02_2MoveY);
            img_star02_2.Refresh();





            int bottomCW = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - lanuage.Size.Width);
            int bottomCH = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height - lanuage.Size.Height);

            lanuage.Location = new Point(bottomCW - 25, bottomCH - 25);
            lanuage.Refresh();
        }

        //----------Ends UI update----


        //Timer to fade arrow in and out on start screen
        private void timer1_Tick(object sender, EventArgs e)
        {
            img_arrow.Image = changeTransparacy(img_arrow.Image, alpha);

            if (inOut == true)
            {
                alpha -= 10;
            }
            if (inOut == false)
            {
                alpha += 10;
            }

            if (alpha < 10)
            {
                inOut = false;
            }
            if (alpha > 240)
            {
                inOut = true;
            }

        }


        private void startGame_Click(object sender, EventArgs e)
        {
            /*
            datalog();
            img_arrow.Visible = false;
            img_UFO.Visible = false;
            startGame.Visible = false;
            Thread.Sleep(300);
            runGame();
            */
            //startGameViaSerial();

        }

        private void startGameViaSerial()
        {

            timer1.Stop();
            img_arrow.Visible = false;
            startGame.Visible = false;
            loadingScreen.Visible = true;
            img_UFO.Location = new Point(img_UFO.Location.X - 20, img_UFO.Location.Y);
            loadingScreen.Refresh();
            loading.Start();

        }

        public void stopGame()
        {

            gameEnd.Play();
            //Title.ForeColor = #483D8B;
            Title.Visible = false;
            img_planet02_2.Refresh();
            img_planet03_2.Refresh();
            img_star01_1.Refresh();
            img_star01_2.Refresh();
            Title.Refresh();
            theScore = rnd.Next(50, 250);
            for (int i = 0; i < 500; i++)
            {

                img_UFO.Location = new Point(img_UFO.Location.X, img_UFO.Location.Y - 1);
                img_UFO.Refresh();
                Thread.Sleep(1);
            }
            Console.WriteLine("end of game");
            gameAudio.Stop();
            openUI();
            System.Windows.Forms.Application.ExitThread();
            this.Close();
            lanSelect = 0;



        }

        public void userInputRecieved()
        {



            var j = Math.Pow(3, (5 - numInstructions)) + 40;
            var mx = 0;
            var my = 0;
            var action = numInstructions % 3;

            if (action == 0)
            {
                my = -1;
                mx = -1;
            }
            else if (action == 2)
            {

                my = 1;
                mx = -1;
            }
            else if (action == 1)
            {
                my = 1;

            }



            img_UFO.Visible = true;

            for (int i = 0; i < j; i++)
            {
                img_UFO.Location = new Point(img_UFO.Location.X + mx, img_UFO.Location.Y + my);
                img_UFO.Refresh();
                Thread.Sleep(1);
            }
            for (int i = 0; i < j; i++)
            {
                img_UFO.Location = new Point(img_UFO.Location.X - mx, img_UFO.Location.Y - my);
                img_UFO.Refresh();
                Thread.Sleep(1);
            }


        }
        /*
                private void ready321()
                {

                    action.Text = readyFReady;
                    action.Visible = true;
                    action.Refresh();


                    for (int t = 3; t > -1; t--)
                    {

                        if (t != 0)
                        {
                            instruction.Text = t.ToString();
                        }
                        else
                        {
                            instruction.Text = readyFGo;
                        }

                        instruction.Visible = true;
                        instruction.Refresh();
                        Thread.Sleep(1000);
                    }

                    instruction.Visible = false;
                    action.Visible = false;
                    action.Refresh();
                    instruction.Refresh();


                }

                //----Debug button----
                //debug button to move straight to user mode skipping game
                private void button1_Click(object sender, EventArgs e)
                {
                    //this.Close();
                    openUI();
                }
                */
        private void openUI()
        {
            thUI = new Thread(opennewform);
            thUI.SetApartmentState(ApartmentState.STA);
            thUI.Start();

        }

        private void opennewform()
        {
            //disable data in and close serial port before changing screen
            if (gameSerial.IsOpen)
            {
                gameSerial.DtrEnable = false;
                gameSerial.Close();
            }

            Application.Run(new UserMode());
        }

        //----End of Debug button code----

        private void lanuage_Click(object sender, EventArgs e)
        {




        }


        public void changeLanguage()
        {

            
            if (lanSelect % 2 == 0)
            {
                //Update lanuage block on the screen
                lanuage.Image = Properties.Resources.EnglishLang;
                lanuage.Refresh();

                //Set Text to language
                startGame.Text = "Start Game";
            }
            if (lanSelect % 2 == 1)
            {
                //Update lanuage block on screen to new lanuage
                lanuage.Image = Properties.Resources.spanishLang;
                lanuage.Refresh();

                //Update text
                startGame.Text = "Empezar juego";

            }

        }

        //open log file userModeLog.txt and write the date/time, score, highscore and games played since startup to the file then close the file
        public void datalog()
        {

            using (StreamWriter logFile = File.AppendText("userModeLog.txt"))
            {

                int Score = getScore();


                if (HIGHSCORE < Score)
                {
                    HIGHSCORE = Score;
                }


                logFile.WriteLine("Game played at {0} {1} with score {2} the current highscore is {3}",
                DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString(), Score, HIGHSCORE);

                logFile.WriteLine("Total games played: {0}", GAMESPLAYED);
                GAMESPLAYED++;
                logFile.Flush();

                logFile.Close();
            }

        }


        private void gameSerial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //give time to allow all data to come into the computer
            Thread.Sleep(2);


            String testing1 = "connected to: ";
            testing1 += gameSerial.ReadExisting().ToString();
            SetText(testing1);


            var distanceSens=0;
            var r =0;
            var g=0;
            var b=0;

            if (r > g && r > b) {
                lanSelect = 2;
            }
            if (r > b && g > b) {
                lanSelect = 3;
            }
            if (numInstructions >= 5 && distanceSens < 30)
            {
                startGameViaSerial();
                numInstructions--;
            }

        }

        delegate void SetTextCallback(string text);
        private object mScanLock = new object();
        private void SetText(string text)
        {
            if (text.Contains("go"))
            {
                lock (mScanLock)
                {
                    UserMode frm = new UserMode();
                    frm.ShowDialog();
                    frm.Dispose();
                }
            }
            try
            {
                /*gameDebugWindow.AppendText(text);
                if (gameDebugWindow.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    gameDebugWindow.Invoke(d, new object[] { text });
                }
                else
                {
                    gameDebugWindow.Clear();
                    gameDebugWindow.Text = text;
                    gameDebugWindow.Refresh();
                    */
                img_arrow.Visible = false;
                //img_UFO.Visible = false;
                startGame.Visible = false;
                Thread.CurrentThread.Abort();
                //runGame();
                //}

            }
            catch
            {
                Console.WriteLine("Threadig error occured");

            }
        }


        //Basically end of start screen code
        private void img_arrow_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void planet01_2_Click(object sender, EventArgs e)
        {

        }

        private void planet04_1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click_2(object sender, EventArgs e)
        {

        }

        private void img_planet04F_1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click_3(object sender, EventArgs e)
        {

        }

        private void StartScreen_KeyPress(object sender, KeyPressEventArgs e)
        {

            //the L key is the equlivant to swapping betweeen a red(for english) and yellow(for spanish block) before the game starts
            if (e.KeyChar == (Char)Keys.L)
            {
                lanSelect++;
                changeLanguage();
            }
            //Equlivant to any input being recieved during the game's operation
            if (e.KeyChar == (Char)Keys.K)
            {
                if (numInstructions >= 5)
                {
                    startGameViaSerial();
                }
                else if (numInstructions <= 0)
                {

                    stopGame();
                }
                else if (numInstructions < 5 || numInstructions > 0)
                {
                    //an instruction from mbed
                    userInputRecieved();
                }

                numInstructions--;


            }
            if (e.KeyChar == (Char)Keys.T) {
                
                Thread tut;
                tut = new Thread(tutorial);
                tut.SetApartmentState(ApartmentState.STA);
                tut.Start();

                gameAudio.Stop();
               

            }
        }

        public void tutorial() {

            Application.Run(new tutorial());


        }

        private void Title_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

        }

        private void loading_Tick(object sender, EventArgs e)
        {
            loadingTick++;
            if (loadingTick == 5)
            {

                loadingScreen.Visible = false;

            }
        }

        private void gameStart_Tick(object sender, EventArgs e)
        {
            if (gameSerial.IsOpen)
            {

                writeRequest("d", 0);
                writeRequest("c", 0);

            }
            else {
                gameStart.Stop();
            }
        }
    }
}
