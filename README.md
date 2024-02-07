Hello,

This program shows you if your internet connection is safe with an active VPN tunnel. That's why it's called VPN Active. The program minimizes to the tray. The tray icon is blue if there is no active internet connection. The icon is red when you have an unsafe internet connection and the icon is green when you have a VPN connection.

How does it work? The program gets your public IP first. With this IP the program does an reverse dns lookup. This result gives you the public name of your public address. For instance. Your internet provider is called "myprovider". Your public address is : 11.22.33.44. The reverse DNS gives 11-22-33-44.myprovider.com. The icon will turn red if If you have "myprovider" in the textbox. Multiple things can be searched. The divider is a ",". So "myprovider,services" will look for "myprovider" and for "services" in your public ip name. In both cases the icon will be red.

Todo: this program only checks the main connection. So there is no problem if you only have 1 connection (lan or WiFi). But only 1 connection will be checked if you have multiple network devices (ethernet and WiFi). We have to change it for checking how many internet connections there are and check if none of these public addresses have a match with the text in the textbox.

I didn't have the time to do this. Maybe you can help.
