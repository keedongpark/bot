using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace loady
{
    /// <summary>
    /// Msg로 전달 받아서 처리     
    /// 
    /// act begin / end 시간을 전부 csv로 남김
    /// - agent, login, time, time, elapsed
    /// - excel에서 리포트로 정리
    /// </summary>
    public class Report
    {
        private static Report report = new Report();

        public static Report Inst()
        {
            return report;
        }

        private Report()
        {
        }

        ConcurrentQueue<Msg> mq = new ConcurrentQueue<Msg>();
        FileStream fs;

        public void Start(string filename)
        {
            fs = File.Open(filename, FileMode.OpenOrCreate);

            // excel doesn't recognize utf8 file automatically. 
            Encoding vUTF8Encoding = new UTF8Encoding(true);
            var vPreambleByte = vUTF8Encoding.GetPreamble();
            fs.Write(vPreambleByte, 0, vPreambleByte.Length);
        }

        public void Execute()
        {
            Msg m;

            while ( mq.TryDequeue(out m) )
            {
                Log(m);
            }
        }

        public void Notify(Msg m)
        {
            mq.Enqueue(m);
        }

        public void Stop()
        {
            Execute(); // write all messages

            fs.Close();
        }

        private void Log(Msg m)
        {
            if ( fs != null)
            {
                var line =  $"{m.json["agent"]}, " +
                            $"{m.json["category"]}, " +
                            $"{m.json["name"]}, " +
                            $"{m.json["begin"]}, " +
                            $"{m.json["end"]}, " +
                            $"{m.json["elapsed"]}\r\n";

                byte[] utfBytes = Encoding.UTF8.GetBytes(line);

                fs.Write(utfBytes, 0, utfBytes.Length);
            }
        }
    }
}
