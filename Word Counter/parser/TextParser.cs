using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Word_Counter.parser
{
    class TextParser : AbstractFileParser
    {
        /** 
         * Ergebnis Speicher         
         */
        Dictionary<string, int> resultData;

        public override async Task<Dictionary<string, int>> parse(System.IO.Stream stream, Form parent, System.Threading.CancellationToken cancellationToken)
        {
            this.resultData = new Dictionary<string, int>();
            StringBuilder builder = new StringBuilder();
            StreamReader reader = new StreamReader(stream);
            byte[] chunk = new byte[1];
            Regex rgx = new Regex(@"\s+");
            long max = stream.Length;
            int updateSteps = (int) Math.Round(Math.Sqrt(max), 0);            

            MemoryStream streamCopy = new MemoryStream();
            stream.CopyTo(streamCopy);

            Label textProgress = parent.Controls.Find("label2", true).FirstOrDefault() as Label;
            Label etaLabel = parent.Controls.Find("etaLabel", true).FirstOrDefault() as Label;
            Label timeLabel = parent.Controls.Find("timeLabel", true).FirstOrDefault() as Label;
            ProgressBar bar = parent.Controls.Find("progressBar2", true).FirstOrDefault() as ProgressBar;

            textProgress.Invoke(new Action(() =>
            {
                textProgress.Text = "0 von " + max + " Zeichen durchsucht.";
            }));

            Stopwatch elapsedTimeWatch = new Stopwatch();
            Stopwatch etaWatch = new Stopwatch();

            elapsedTimeWatch.Start();
            etaWatch.Start();
            streamCopy.Position = 0;

            for (int pos = 0; pos < streamCopy.Length; pos++)
            {
                if (pos % updateSteps == 0 && pos != 0)
                {
                    // ETA TIME
                    etaWatch.Stop();
                    long streamLeft = max - pos;
                    double mLeft = (streamLeft / updateSteps) * etaWatch.ElapsedMilliseconds;
                    TimeSpan tLeft = TimeSpan.FromMilliseconds(mLeft);

                    etaLabel.Invoke(new Action(() =>
                    {
                        etaLabel.Text = "Verbleibend: " + String.Format("{0:00}h {1:00}m {2:00}s",
                        tLeft.Hours, tLeft.Minutes, tLeft.Seconds);
                    }));
                    etaWatch.Restart();

                    // TOTAL TIME
                    TimeSpan ts = elapsedTimeWatch.Elapsed;
                    string elapsedTimeString = String.Format("{0:00}:{1:00}:{2:00}",
                        ts.Hours, ts.Minutes, ts.Seconds);
                    timeLabel.Invoke(new Action(() =>
                    {
                        timeLabel.Text = "Dauer: " + elapsedTimeString;
                    }));

                    // Progress Update
                    int progress = (int)(100 * pos / streamCopy.Length);
                    if (progress > 100) progress = 100;
                    if (progress < 0) progress = 100;

                    bar.Invoke(new Action(() =>
                    {
                        bar.Value = progress;
                    }));

                    textProgress.Invoke(new Action(() =>
                    {
                        textProgress.Text = (pos + 1) + " von " + max + " Zeichen durchsucht.";
                    }));
                }

                streamCopy.Read(chunk, 0, 1);

                if (rgx.IsMatch(System.Text.Encoding.UTF8.GetString(chunk)))
                {
                    this.insertWord(builder);
                }
                else
                {
                    builder.Append(System.Text.Encoding.UTF8.GetString(chunk));
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            textProgress.Invoke(new Action(() =>
            {
                textProgress.Text = max + " von " + max + " Zeichen durchsucht.";
            }));

            elapsedTimeWatch.Stop();

            if (builder.Length > 0)
            {
                insertWord(builder);
            }

            bar.Invoke(new Action(() =>
            {
                bar.Value = 100;
            }));

            System.Threading.Thread.Sleep(500);
            return this.resultData;
        }

        void insertWord(StringBuilder builder)
        {
            string newItem = builder.ToString();
            Regex rgx = new Regex(@"\s+");
            if (builder.Length == 0)
            {
                builder.Clear();
                return;
            }

            if (!this.resultData.ContainsKey(newItem))
            {
                this.resultData[newItem] = 1;
            }
            else
            {
                this.resultData[newItem]++;
            }

            builder.Clear();
        }
    }
}
