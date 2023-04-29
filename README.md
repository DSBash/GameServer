# GameServer
Client / Server application with public broadcast and private send methods. : [GameServer](https://github.com/DSBash/GameServer)

## Features
* Single WinForm Client/Server Interface
* Custom IP and Port Specifications
* Synchronous and ASynchronous TCP Connections
* JSON Object Formatting
* AES Encryption
* Key/Password Authorization
* Multiplayer Drawing / Whiteboard
* Public and Private Chats w/ Colour (Color Controls by: [Copyright (c) 2012, Yves Goergen](http://unclassified.software/) )
* Client list (click to add '/msg clientname' for PM)
* Sent Command/Message History

### Chat and Commands
Public and Private messages that use colour for easy reading. Messages have Date and Time pre-fix for refrence.
Features or Routines can be activated by typing them into the 'Send Message Box' such as:
#### Shared
>*  /msg UserName Message
>*  /save = Saves the current Drawing to file
>*  /export = Saves the current tabs Text box contents to file
#### Host only
>*  /send = Test feature to have clients become file server to receive current Host Drawing
#### Client only
>*  /picme = get new copy of the current Host Drawing

### Drawing / WhiteBoard
Connected clients can participate in the drawing, creating interesting group pictures that can be saved.
Draw freehand or with shapes such as Circle, Triangle, and Rectangle.
Fill option works great with Shapes and 'Pen w/ Close' (filling the whole image can cause issues)

### Interface
![PIC1](https://github.com/DSBash/GameServer/blob/master/Server/IMG/1.png?raw=true)


### Known Issues
- Fill tool causes out of memory error at times, carefull filling manual 'Pen' drawing and/or the open Canvas.

### To Do:
- add text image to drawings
- debug mode
- transparent drawing canvas to show game below
- incorporate with : [GridGame](https://github.com/DSBash/GridGame)
