using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UIwithTimer
{

    //public delegate void ReadDele();
    public class ReadTask
    {
        //public event ReadDele ReadStopThreadEvent = null;

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();    //to lock file during write process

        public string Name { get; set; }
        public int Runtime { get; set; }
        public int Second { get; set; }
        public int Samplingrate { get; set; }

        public List<string> times = null;
        public List<string> svs = null;
        public List<string> pvs = null;

        public ReadTask(string name, int runtime, int samplingrate)
        {
            Name = name;
            Runtime = runtime;
            //Second = second;
            Samplingrate = samplingrate;
        }

        public void Run()
        {
            times = new List<string>();
            svs = new List<string>();
            pvs = new List<string>();
            string lines = null;
            char[] delimiterChars = { ' ', ',', '\r', '\n' };   //tokenize the words

            for (int i = 0; i < Runtime; i++)
            {
                if (i % Samplingrate == 0)
                {
                    lines += GetLastLine("SVPV.txt", i) + "\r\n";
                }
            }


            //string lastline = GetLines("SVPV.txt", Runtime-Second);

            //ConsoleManager.Show();
            //Console.WriteLine(lastline);

            string [] words = lines.Split(delimiterChars);
            foreach (var word in words)
            {
                if (word.Contains("time=")) //goes into times list
                {
                    times.Add(word.Replace("time=",""));
                }

                if (word.Contains("SV="))   //goes into svs list
                {
                    svs.Add(word.Replace("SV=",""));
                }

                if (word.Contains("PV="))   //goes into pvs list
                {
                    pvs.Add(word.Replace("PV=",""));
                }
            }
        }
        
        /// <summary>
        /// Read Lines from start to end.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public string GetLines(string fileName, int start)
        {
            using (var sr = new StreamReader(fileName))
            {
                for (int i = 1; i < start; i++)
                {
                    sr.ReadLine();
                }
                return sr.ReadToEnd(); 
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
            try
            {
                using (var sr = new StreamReader(fileName))
                {
                    for (int i = 0; i < line; i++)
                    {
                        sr.ReadLine();
                    }
                    return sr.ReadLine();
                }
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
