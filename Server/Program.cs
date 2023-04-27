using System;
using System.Windows.Forms;

namespace Server
{
    static class Program
    {
        public static GameServer mainForm { get; set; }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainForm = new("Host");
            Application.Run(mainForm);

        }
    }
}
