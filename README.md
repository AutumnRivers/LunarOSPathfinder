# LunarOSv3 (aka LunarOSPathfinder)
Moonshine is back, and this time, it's personal.

---

LunarOSv3 is a mod designed for [Hacknet: Pathfinder](https://github.com/Arkhist/Hacknet-Pathfinder). (Specifically, version 5.3.0).

It adds new daemons, new ports, and a new executable.

---

## Daemons

### `<LunarOSDaemon Version="string" [Subtitle="string" TitleXOffset="int" TitleYOffset="int" SubXOffset="int" SubYOffset="int" ButtonText="string"] />`
A mostly-static daemon for LunarOS devices. If you plan to use this in your own extension, you *need* to have a file titled `LunarOSLogo.png` in the `/Images` folder of your extension.

* `Version="string"` - Displays as "LunarOS v(Version)" in the daemon. For example, if you put `Version="6.9-beta", it would show up as "LunarOS v6.9-beta".
* `Subtitle="string"` - (OPTIONAL) Text to show under "LunarOS v...". If left empty, it simply won't render.
* `TitleXOffset/TitleYOffset` - (OPTIONAL) X/Y offset from the center of the daemon for the Version text. (LunarOS v...)
* `SubXOffset/SubYOffset` - (OPTIONAL) Same as above, but for the Subtitle.
* `ButtonText=string` - (OPTIONAL) Text for the button on the top left. Defaults to "Debug Menu".

### `<VaultDaemon [Name="string" FlagPrefix="string" SecretCode="string" MaximumKeys="int"] />`
* `Name="string"` - The big words you see at the top of the daemon. Defaults to "TsukiVault"
* `FlagPrefix="string"` - Keys are tracked via flags. If your `FlagPrefix` is `moonshine`, then key 1 would be `moonshine1`, key 2 would be `moonshine2`, and so on. Defaults to "vault"
* `SecretCode="string"` - Shows on the access granted button, and also acts as the admin password. Defaults to 7 randomly generated alphanumeric characters.
* `MaximumKeys="int"` - How many keys the vault has. Defaults to 5.

---