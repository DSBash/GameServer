using System;
using System.Windows.Forms;

namespace Server
{
    static class Program
    {
        public static GameServer MainForm { get; set; }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new(/*"Host"*/);
            Application.Run(MainForm);

        }
    }
}
