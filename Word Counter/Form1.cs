using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Word_Counter.parser;
using System.Threading;


namespace Word_Counter
{
    public partial class Form1 : Form
    {
        Word_Counter.parser.AbstractFileParser fileParser = new TextParser();
        CancellationTokenSource source;

        public Form1()
        {
            InitializeComponent();
        }

        public void consoleInfo(string msg)
        {
            DateTime today = DateTime.Now;
            textBox1.AppendText(today.ToString(" [HH:mm] "));
            textBox1.AppendText(msg);
            textBox1.AppendText("\n");
        }

        public void consoleNewline()
        {
            textBox1.AppendText("\n");
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            Stream myStream = null;
            if ((myStream = openFileDialog1.OpenFile()) != null)
            {
                using (myStream)
                {
                    fileNameTextBox.Text = openFileDialog1.FileName;
                    startParseButton.Enabled = true;
                    consoleNewline();
                    consoleInfo("Datei wurde ausgewählt: " + openFileDialog1.FileName);
                    consoleInfo("Sie können jetzt mit der Auswertung beginnen.");
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private async void startParseButton_Click(object sender, EventArgs e)
        {
            try
            {
                Stream myStream = null;
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    stopParseButton.Enabled = true;
                    startParseButton.Enabled = false;
                    fileOpenButton.Enabled = false;
                    progressBar2.Value = 0;

                    consoleNewline();
                    consoleInfo("Die Datei wird ausgewertet: " + openFileDialog1.FileName);
                    consoleInfo("Bitte haben Sie einen Moment Geduld. Die Verarbeitung kann einige Zeit in Anspruch nehmen.");

                    using (myStream)
                    {
                        this.source = new CancellationTokenSource();
                        Task<Dictionary<string, int>> task = Task.Run(() => this.fileParser.parse(myStream, this, this.source.Token), this.source.Token);

                        await task;
                        resetButtons(false);
                        Form2 resultForm = new Form2();
                        resultForm.showResult(task.Result);
                        resultForm.ShowDialog();
                    }
                }
            }
            catch (System.OperationCanceledException ex)
            {
                resetButtons();
            }
            catch (System.OutOfMemoryException ex)
            {
                consoleInfo("Fehler: Die Datei ist zu groß zum einlesen.");
                MessageBox.Show("Die Datei ist zu groß zum einlesen.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);                
                resetButtons();
            }
            catch (Exception ex)
            {
                consoleInfo("Fehler: " + ex.Message);
                MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);                
                resetButtons();
            }
        }

        private void resetButtons(bool cancelMessage = true)
        {
            stopParseButton.Enabled = false;
            startParseButton.Enabled = true;
            fileOpenButton.Enabled = true;

            if (cancelMessage)
            {
                consoleInfo("Die Auswertung wurde abgebrochen.");
            }
            else
            {
                consoleInfo("Die Auswertung ist abgeschlossen.");
            }
        }

        private void stopParseButton_Click(object sender, EventArgs e)
        {
            this.source.Cancel();
        }
    }
}
