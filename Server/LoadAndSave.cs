using System;

namespace Server {
    public partial class GameServer {
        private class CustomizationSettings                                                         // Details save to and loaded from File 
        {
            public string UserName { get; set; }
            public string UserColor { get; set; }
            public string RoomKey { get; set; }
            public string Address { get; set; }
            public string Port { get; set; }
            public string ThemeName { get; set; }
            public bool IsDarkModeEnabled { get; set; }
            public DateTime LastLogin { get; set; }
        }

        #endregion
    }
}
/*
 * Snips
 
if (listening) { ;}
else if (connected) { ;}





*/