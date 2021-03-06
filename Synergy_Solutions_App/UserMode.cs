﻿using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
//using System.Windows.Media;

namespace Synergy_Solutions_App
{

    public partial class UserMode : Form
    {
        System.Media.SoundPlayer scoreAudio = new System.Media.SoundPlayer(Properties.Resources.Magical_Triumph1);


        //setting up. scorePH should be changed to the player's score but it is set to 5555 for now
        public int scorePH = StartScreen.theScore;
        int letterChoice = 0;
        public int[] highScoresList = { 101, 100, 99, 95, 80, 50, 10, 7, 9, 2, 1 };
        public string[] highScoreNames = { "STU_", "JO__", "GORI", "ALEX", "BER_", "AAAA", "_ZED", "CATS", "ROBB", "BEAR", "ALI_" };
        public byte AA = 255;
        private object locker = new object();
        int lanFromStartScreen = StartScreen.lanSelect;
        Thread th;


        //getting serial ports (modified from Bernard's code(maintanceMode), modified to auto connect if a port exists)
        public void getSerialPorts()
        {
            string[] ports;
            ports = SerialPort.GetPortNames();



            try
            {
                UISerial.PortName = ports[0];
                UISerial.Open();
                UISerial.DtrEnable = true;
                Console.WriteLine("connected to:" + UISerial.PortName + Environment.NewLine);
            }
            catch
            {
                Console.WriteLine("can't connect to serial bus" + Environment.NewLine);
                //System.Windows.Forms.MessageBox.Show("Cannot connect to system please contact park staff");
            }

        }
        public UserMode()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
           // 
                
            this.Location = new Point(this.Width / 2, (this.Height / 2)-10);


            scoreAudio.PlayLooping();
            lanuage();
            string[] documentLines = File.ReadAllLines("score.txt");
            bool inputChangeOver = true;
            for (int inputData = 0; inputData < 23; inputData++) {
                if (documentLines[inputData] == "/n") {
                    inputChangeOver = false;
                    continue;
                }
                if (inputChangeOver)
                {
                    highScoreNames[inputData] = (documentLines[inputData]);
                }
                else {

                    highScoresList[inputData - 12] = Int32.Parse(documentLines[inputData]);
                    //Console.WriteLine(documentLines[inputData]);

                }
                // Console.WriteLine(inputData);
            }




            //box holding the player's score is loaded at the end of the game as well as the high scores of other users
            scoreText.ReadOnly = true;
            scoreText.Text = scorePH.ToString();

            getSerialPorts();

            for (int k = 0; k <= 10; k++)
            {
                highScores.Items.Add(highScoreNames[k] + "         " + highScoresList[k].ToString());
            }

        }

        private void maintainceModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //this.Close();
            th = new Thread(opennewform);
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

        private void opennewform()
        {
            if (UISerial.IsOpen)
            {
                UISerial.DtrEnable = false;
                UISerial.Close();

            }
            Application.Run(new Authorization());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //can only be pressed once
            button1.Enabled = false;
            submitScore();

            
        }

        public void submitScore() {

            //initializing everything 
            int listLength = highScoresList.Length;
            string userName = " ";

            //on click read in all the letters and write them into the user name variable
            string[] letterList = { letter1.Text, letter2.Text, letter3.Text, letter4.Text };

            for (int i = 0; i < 4; i++)
            {
                userName = userName + letterList[i];
            }



            //go down the high scores and check to see if the player's score is a high score if it is add it to the leader board in
            //the correct place (PLAYER NAME       PLAYER SCORE)
            for (int j = 0; j < listLength; j++)
            {
                //highScoresList[j] = 22;
                if (scorePH >= highScoresList[j])
                {

                    for (int l = 1; l < (listLength - j); l++)
                    {
                        highScoresList[listLength - l] = highScoresList[listLength - 1 - l];
                        highScoreNames[listLength - l] = highScoreNames[listLength - 1 - l];
                        // = -2;
                    }


                    highScoresList[j] = scorePH;
                    highScoreNames[j] = userName;
                    break;
                }




            }


            //update the scoreboard to show the new order
            highScores.Items.Clear();

            for (int k = 0; k <= 10; k++)
            {

                string nameAndScoreTogether = highScoreNames[k] + "         " + highScoresList[k].ToString();

                highScores.Items.Add(nameAndScoreTogether);
                highScores.Refresh();


            }
            datalog();


            Thread.Sleep(3000);
            th = new Thread(loopback);
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            System.Windows.Forms.Application.ExitThread();
            scoreAudio.Stop();
            this.Close();

        }

        private void loopback()
        {
            Application.Run(new StartScreen());
        }


        public void datalog()
        {
            File.WriteAllText("score.txt", string.Empty);
            using (StreamWriter scoreFile = File.AppendText("score.txt"))
            {
                int hiAmount = 11;

                //scoreFile.WriteLine()
                for (int i = 0; i < hiAmount; i++)
                {
                    scoreFile.WriteLine(highScoreNames[i]);

                }

                scoreFile.WriteLine("/n");

                for (int j = 0; j < hiAmount; j++)
                {
                    scoreFile.WriteLine(highScoresList[j]);

                }

                scoreFile.Flush();
                scoreFile.Close();

            }



        }

        public void letterDown(){

            switch (letterChoice)
            {

                case 0:
                    letter1.DownButton();
                    break;
                case 1:
                    letter2.DownButton();
                    break;
                case 2:
                    letter3.DownButton();
                    break;
                case 3:
                    letter4.DownButton();
                    break;
                default:
                    Console.WriteLine("out of letters");
                    letterChoice = 0;
                    break;


            }
        }

        public void letterUp() {

            switch (letterChoice)
            {

                case 0:
                    letter1.UpButton();
                    break;
                case 1:
                    letter2.UpButton();
                    break;
                case 2:
                    letter3.UpButton();
                    break;
                case 3:
                    letter4.UpButton();
                    break;
                default:
                    Console.WriteLine("out of letters");
                    letterChoice = 0;
                    break;


            }

        }



        //button to move through letters in the options box (will be implimented into a hardware button soon)
        private void button2_Click(object sender, EventArgs e)
        {
            letterUp();
        }
        //button to move through letters in the options box (will be implimented into a hardware button soon)
        private void debugButton2_Click(object sender, EventArgs e)
        {
            letterDown();
        }
        //button to move into the next letter selection
        private void button2_Click_1(object sender, EventArgs e)
        {
            lanuage();
           
        }

        private void lanuage()
        {

            
            if (lanFromStartScreen%2 == 1)
            {
                label3.Text = "Name";
                button1.Text = "Submit";
                label2.Text = "High Score!";
                label4.Text = "Your Score";

            }
            if (lanFromStartScreen%2 ==0)
            {

                label3.Text = "Nombre";
                button1.Text = "Enviar";
                label2.Text = "Alto Puntuación!";
                label4.Text = "Tu Puntuación";

            }

        }

        private void UISerial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }

        private void UserMode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (Char)Keys.D1)
            {
                letterUp();
            }
            else if (e.KeyChar == (Char)Keys.D2)
            {
                letterDown();
            }
            else if (e.KeyChar == (Char)Keys.D3)
            {
                if (letterChoice < 4)
                {
                    letterChoice++;
                }
                else {
                    submitScore();
                }

            }
        }



        // The goal of this is to fade in and out but alas it doesn't
        /*           private void timer1_Tick(object sender, EventArgs e)
                   {
                       changeTransparacy(AlienImage.Image, alienAlpha);
                       alienAlpha-=20;
                   }


           public void runTimer()
           {
               System.Timers.Timer aTimer = new System.Timers.Timer(10000);

               aTimer.Elapsed += new ElapsedEventHandler(RunEvent);
               aTimer.Interval = 2;
               aTimer.Enabled = true;
               if (AA == 0)
               {
                   aTimer.Stop();
               }
           }

           //This method will get called every second until the timer stops or the program exits.
           public void RunEvent(object source, ElapsedEventArgs e)
           {

               Image ph = AlienImage1.Image;
               lock (locker)
               {


                   if (AA >= 45 )
                   {
                       AA -= 30;

                       ph = changeTransparacy(ph, AA, ph.Width, ph.Height);
                       AlienImage1.Image = ph;


                   }
                   if(AA <=45)
                   {
                       ph = changeTransparacy(ph, 0, ph.Width, ph.Height);
                       AlienImage1.Image = ph;
                       AA = 0;

                   }
               }



           }

           /*
   private void AlienImage_MouseEnter(object sender, EventArgs e)
   {
      /*

      DoubleAnimation da = new DoubleAnimation
      {
          From = 0,
          To = 1,
          Duration = new Duration(TimeSpan.FromSeconds(1)),
          AutoReverse = true
      };
      Logo.BeginAnimation(OpacityProperty, da);


      Image ing = (Image)sender;
      DoubleAnimation animate = new DoubleAnimation(0, TimeSpan.FromSeconds(2));
      ing.BeginAnimation(Opacity, animate);


     /// this.Fade(value: 0.5f, duration: 2500, delay: 250, easingType: EasingType.Default).Start();
     // await this.Fade(value: 0.5f, duration: 2500, delay: 250, easingType: EasingType.Default).StartAsync();  //Fade animation can be awaited
     */
    }
}
//}
