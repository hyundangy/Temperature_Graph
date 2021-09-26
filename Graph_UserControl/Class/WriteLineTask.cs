using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Graph_UserControl.Class
{
    public class WriteLineTask
    {
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();    //to lock file during write process
        //public event ReadDele WritestopinvokeEvent = null;

        public bool IsReading = false;
        public bool IsWriting = false;

        public string Name { get; set; }
        public int Runtime { get; set; }    
        public double SV { get; set; }
        public double PV { get; set; }
        public double tempSV { get; set; }
        public int checkstart { get; set; }
        public int troublesim { get; set; }

        public WriteLineTask(string name, int runtime, double sv, double pv)
        {
            Name = name;
            Runtime = runtime;
            SV = sv;
            PV = pv;
            Createtxt();
            FileStream file = new FileStream("SVPV.txt", FileMode.Append, FileAccess.Write, FileShare.Read);
            file.Close();
        }

        internal void Run()
        {
            //SV = (int)(100 * Math.Sin((double)Runtime * 0.002));
            //PV = (int)(100 * Math.Cos((double)Runtime * 0.002));

            if (PV < SV && troublesim == 0)
            {
                PV += 0.1;
                PV = Math.Round(PV * 10) / 10;
            }

            else if (PV > SV && troublesim == 0)
            {
                PV -= 0.1;
                PV = Math.Round(PV * 10) / 10;
            }

            else if (PV == SV && troublesim == 0)
            {
                TroubleSim();
            }

            if (tempSV > PV && troublesim == 1)
            {
                PV += 0.1;
                PV = Math.Round(PV * 10) / 10;
            }
            else if (tempSV < PV && troublesim == 1)
            {
                PV -= 0.1;
                PV = Math.Round(PV * 10) / 10;
            }
            else if (tempSV == PV && troublesim == 1)
            {
                troublesim = 0;
            }

            //while (IsReading);

            Writetxt("SVPV.txt", Runtime.ToString(), SV.ToString(), PV.ToString());

            Runtime++;
        }

        private void TroubleSim()
        {
            Random rand = new Random();
            int random = rand.Next(0, 2);

            if (random == 0)
            {
                tempSV = rand.Next((int)SV+5, (int)SV + 7);
            }
            else
            {
                tempSV = rand.Next((int)SV - 7, (int)SV - 5);
            }

            troublesim = 1;
        }

        public void Writetxt(string filename, string time, string s, string p)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                IsWriting = true;
                using (StreamWriter sw = new StreamWriter(filename, true, Encoding.UTF8))
                {
                    string NextLine = string.Format("time={0},SV={1},PV={2}\r\n", time, s, p);
                    sw.Write(NextLine);
                    sw.Close();
                }
                IsWriting = false;
            }
            catch (IOException ex)
            {
                //ConsoleManager.Show();
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }

            //StreamWriter sw = new StreamWriter(filename, true, Encoding.UTF8);
            //string NextLine = string.Format("time={0},SV={1},PV={2}\r\n", time, s, p);
            //sw.Write(NextLine);
            //sw.Close();
        }

        public FileStream Createtxt()
        {
            FileStream txtfile = new FileStream("SVPV.txt", FileMode.Create); //creating file stream
            txtfile.Close();

            return txtfile;
        }


    }
}
