using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UIwithTimer
{

    public delegate void CurrDele(int runtime);

    public class CurrTask : INotifyPropertyChanged
    {
        //public event CurrDele CurrinvokeEvent = null;
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();    //to lock file during write process

        Thread t;
        public string Name { get; set; }
        public int runtime = 0;
        public int Runtime
        {
            get { return this.runtime; }
            set
            {
                if (this.runtime != value)
                {
                    this.runtime = value;
                    this.NotifyPropertyChanged("Runtime");
                }
            }
        }

        public string time;
        public string sv;
        public string pv;
        //public string [] words = null;

        //char[] delimiterChars = { ' ', ',', '\r', '\n' };   //tokenize the words

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public CurrTask (string name, int runtime)
        {
            Name = name;
            Runtime = runtime;
        }

        public void Run(int runtime1)
        {
            //words = new string[10];
            char[] delimiterChars = { ' ', ',', '\r', '\n' };   //tokenize the words

            //read starts
            string lastline = GetLastLine("SVPV.txt", runtime1);
            //read ends (이 사이에 write 못하게 막아야함)

            //ConsoleManager.Show();
            //Console.WriteLine(lastline);

            string[] words = lastline.Split(delimiterChars);
            foreach (var word in words)
            {
                if (word.Contains("time=")) //goes into times list
                {
                    time = (word.Replace("time=", ""));
                }

                if (word.Contains("SV="))   //goes into svs list
                {
                    sv = (word.Replace("SV=", ""));
                }

                if (word.Contains("PV="))   //goes into pvs list
                {
                    pv = (word.Replace("PV=", ""));
                }
            }

        }

        /// <summary>
        /// Read the last line
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public string GetLastLine(string fileName, int line)
        {
            _readWriteLock.EnterReadLock();
            string s;
            try
            {
                using (var sr = new StreamReader(fileName))
                {
                    for (int i = 2; i < line; i++)
                    {
                        sr.ReadLine();
                    }
                    s = sr.ReadLine();
                    sr.Close();
                }

                return s;
            }
            catch (IOException ex)
            {
                ConsoleManager.Show();
                Console.WriteLine(ex.ToString());
                Console.WriteLine("line:" + line);
                return "";
            }
            finally
            {
                _readWriteLock.ExitReadLock();
            }

        }



    }
}
