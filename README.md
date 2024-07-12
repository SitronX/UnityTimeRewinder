</p>
<p align="center">
  <img src="https://user-images.githubusercontent.com/68167377/196243323-3509d486-b9dd-4575-9753-134ab1b39de2.png"/>
</p>

# Time Rewinder for Unity

Rewind time in Unity with ease! While there are certain frameworks that let you rewind time in Unity, they are usually quite restrictive and hard to modify. I faced these problems myself when i needed to add time rewind mechanics in my game and none of the solutions that i have found were good enough. That is why i decided to start this open source project that i hope you find usefull. Time Rewinder uses highly efficient circular buffer implementation to store and retrieve the rewinded values.

**Customizability is one of the main points of this project, so it can be used in any of your custom Unity projects and you can track and rewind anything you want, even Shaders!**

Straight from the box you can start rewinding **Object active states, Transforms (position, rotation, scale), velocities, animators (also all animator layers and parameters), audios and particle systems**.

## How to install

You have two options how to install it into your project.

- First option is to download prepared Unity package in Github release section. After download, open your Unity project and simply open the downloaded package, import dialogue window should appear (Unity should automatically associate the package with itself). If it didnt work out for you, you can also import it thru Unity package manager.
- Second option is to download this sample project from Github and start using it, or just import the TimeRewinder folder under Assets/TimeRewinder into your project

<b>Note:</b> Unity versions 2019+ are officially supported

## Features

TimeRewinder supports two types of rewinds.

- Instant rewinds where you rewind time by specified amount of seconds
- Rewinds with previews

The latter of the two is definitely more interesting option, cause you can freely choose which snapshot you can return to after you spectate these previews for yourself. This is especially helpfull if you want to give more control to player with rewinding time. As shown in demo-scene examples, player can choose on the time axis the exact moment he wants to return to, while the tracked attributes are paused. The showcase of this mechanic is shown right below. I think, this is also unique feature of this project, because i havent found a similar functionality in other Time Rewind frameworks.

![ezgif com-gif-maker (9)](https://user-images.githubusercontent.com/68167377/196203578-a476d5b1-5314-49bd-933d-904eba1dd51a.gif)

The classic functionality to rewind time by holding button, which you probably already know from other solutions is also here. These two types of rewinds inputs are prepared straight from the box, but it would be very easy to design completely new rewind input system. You would only have to call corresponding methods, that are all prepared and documented for you.

![ezgif com-gif-maker (10)](https://user-images.githubusercontent.com/68167377/196241351-b1c05483-79e1-4554-8fc2-d4f6efc69b14.gif)


## How to use

Detailed steps how to use TimeRewinder are described in [documentation](https://github.com/SitronX/UnityTimeRewinder/blob/main/Assets/TimeRewinder/Documentation/Unity%20Time%20Rewinder.pdf) and all important parts of code are also documented.

If you still face any problem, feel free to contact me, i can help you out.

## Showcase of rewinding


In demo-scenes there are few examples of time rewinding, as well as two examples of tracking and rewinding custom variables. I recommend you look into it, so you get the idea how everything is connected. Here comes few other videos showcasing rewinding time from demo scenes



https://user-images.githubusercontent.com/68167377/196215651-c3002e8b-a722-4bb6-b655-81946cfeff18.mp4

https://user-images.githubusercontent.com/68167377/196215735-e3e612a4-aa69-40e2-8e4f-3fd68669b667.mp4

https://github.com/SitronX/UnityTimeRewinder/assets/68167377/a4e742ff-1bd2-4108-a6bb-eed39d1c62e6

https://user-images.githubusercontent.com/68167377/196240813-ba4c6b79-ebec-461e-9bbe-335cc75a7af7.mp4

https://github.com/user-attachments/assets/5c8473c5-e45a-4ea2-bc36-22b67b658afe

And here is example and maybe motivation how it could look in actual game (mobile game), where you customize it for your needs :)


https://user-images.githubusercontent.com/68167377/196215814-45e4667b-748a-4eff-a2ce-8c62b9d90f29.mp4

## Coverage

Here you can watch nice video by SpeedTutor covering this asset

[![Watch the video](https://img.youtube.com/vi/4BiPytgJmVo/maxresdefault.jpg)](https://www.youtube.com/watch?v=4BiPytgJmVo)

## License

This asset is completely free under the [MIT license](https://github.com/SitronX/UnityTimeRewinder?tab=MIT-1-ov-file), so you can use it as you wish :)

If you like my projects, please consider buying me a coffee to support my work. All donations will be greatly appreaciated :)

[![BuyMeACoffe](https://github.com/user-attachments/assets/d02ae4a8-3c28-4b01-8b15-807871893856)](https://buymeacoffee.com/sitronx)

## Shameless plug

The main propeler of this project is my released game where you can find this rewind system in practice. This physics based puzzle game is available for free on [Google Play](https://play.google.com/store/apps/details?id=com.SitronCOR.Forcel).

[![GooglePlay](https://github.com/SitronX/UnityTimeRewinder/assets/68167377/0723f83b-cb70-4777-a85d-67a7bb138696)](https://play.google.com/store/apps/details?id=com.SitronCOR.Forcel)

<b>Thank you if you decide to try it out :)</b>
