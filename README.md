# GameServer
Client / Server application with public broadcast and private send methods. : [GameServer](https://github.com/DSBash/GameServer)

## Features
* Single WinForm Client/Server Interface
* Custom IP and Port Specifications
* Synchronous and Asynchronous TCP Connections
* JSON Object Formatting using [Newtonsoft.Json](https://www.newtonsoft.com/)
* AES Encryption for secure transmitions
* Key/Password and Unique-name Authorizations
* Multiplayer Drawing / Whiteboard
* Public and Private Chats w/ Colour (Color Controls by: [Copyright (c) 2012, Yves Goergen](http://unclassified.software/) )
* Client list & ChatTab can be clicked to add '/msg clientname' for PMs
* Command Line History (up/down arrows)
* Save/Load application settings to file
* Exportable Console and Message logs

### Chat and Commands
Public and Private messages that use colour, and conversations in Tabs for easy reading. Messages have Date and Time pre-fix for refrence.
Features or Routines can be activated by typing them into the 'Send Message Box' such as:
#### Shared
>*  /msg UserName Message
>*  /save = Saves the current Drawing to file
>*  /export = Saves the current tabs Text box contents to file
>*  /darkmode <on|off|null>
#### Host only
>*  /send = Test feature to have clients become file server to receive current Host Drawing
#### Client only
>*  /picme = get new copy of the current Host Drawing

### Drawing / WhiteBoard
Connected clients can participate in the drawing, creating interesting group pictures that can be saved.
Draw freehand or with shapes such as Circle, Triangle, and Rectangle.
Fill option works great with Shapes and 'Pen w/ Close' (filling the whole image can cause issues)
Changing the BackGround performs a color replace using x,y iteration.
Transparancy and True-Transparancy to see items below.

### Interface
![PIC1](https://github.com/DSBash/GameServer/blob/master/Server/IMG/1.png?raw=true)


### Known Issues
- Fill tool causes out of memory error (carefull filling manual 'Pen' drawing and/or the open Canvas)

### To Do:
- add imagetext to drawings
- flood control
- debug mode(s)
- incorporate with : [GridGame](https://github.com/DSBash/GridGame)
