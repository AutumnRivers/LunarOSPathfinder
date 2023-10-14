# LunarOSv3 (aka LunarOSPathfinder)
Moonshine is back, and this time, it's personal.

---

LunarOSv3 is a mod designed for [Hacknet: Pathfinder](https://github.com/Arkhist/Hacknet-Pathfinder). (Specifically, version 5.3.0).

It adds new daemons, new ports, and other things.

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
A dynamic daemon that refuses to let the player gain administrator access until they find all the keys. Imagine it like... a layered whitelist. Except way cooler.

* `Name="string"` - The big words you see at the top of the daemon. Defaults to "TsukiVault"
* `FlagPrefix="string"` - Keys are tracked via flags. If your `FlagPrefix` is `moonshine`, then key 1 would be `moonshine1`, key 2 would be `moonshine2`, and so on. Defaults to "vault"
* `SecretCode="string"` - Shows on the access granted button, and also acts as the admin password. Defaults to 7 randomly generated alphanumeric characters.
* `MaximumKeys="int"` - How many keys the vault has. Defaults to 5.

---

## Actions

### `<DebugCheck [Name="string"] />`
A simple action that checks if the user is in debug mode. Sends a message to the user's terminal when fired and creates a file on their node titled `DebugModeEnabled`.

* `Name="string"` - The name to show in the terminal. If omitted, defaults to "Autumn"
	* Call it a big ego, but this *was* made for *my* extension, so... :)

### `<LaunchLunarDefender [Force="bool"] />`
Launches LunarDefender for the player.

* `Force="bool"` - Whether or not to force LunarDefender onto the player. If enabled, will kill every other program, and launch LunarDefender on extension load so long as "KeepLDActive" flag is kept applied. Defaults to "True".

### `<KillLunarDefender />`
Kills LunarDefender for the player, and removes the `KeepLDActive` flag if it was applied.

### `<WriteToTerminal [Quietly="bool"]>Message</WriteToTerminal>`
Writes a message to the terminal a la `writel`. Setting `Quietly` to `"false"` will flash the screen when the message is sent.

---

## Ports

* `3653` - Moonshine Services
* `7600` - LunarDefender (for LunarOS systems)
* `1961` - ??? (Spoilers!)

---

## Executables

### `LunarDefender`
Cannot be launched nor killed by the user. Protects the user from a ForkBomb completing, then has a 10 minute cooldown.

### `LunarEclipse`
`$ LunarEclipse 3653 [-s 22|-r 554]`   
`#LUNAR_ECLIPSE#`

Port cracker for Moonshine Services. Requires SSH or RTSP to be open.

### `Armstrong`
`$ Armstrong [-l]`   
`#ARMSTRONG_EXE#`

<details>
<summary>Extension Spoilers!</summary>

Crashes the LunarDefender service, temporarily opening a port on certain nodes. When LunarDefender relaunches, it will reboot the system.
</details>

<details>
<summary>Extension Spoilers!</summary>

`#ARMSTRONG_PLAYER_EXE#`

Crashes LunarDefender for the player. Plays a fancy animation then runs the equivalent of the `<KillLunarDefender />` action.
</details>