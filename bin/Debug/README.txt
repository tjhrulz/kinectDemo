Overview
-------------------------------------------------------------------------------------------------------------------

Run the kinectDemo executable to begin the demo. You will be greated
with either a series of images or a white screen depending on the
configurations of the loadInstructions file. 

From left to right, then top to bottom they display the folliwing:
1.Color image from the Kinect's camera
2.Colored Image from the depth sensor, blue is getting to close, red
	is getting to far, green is in a good range, white is unknown
3.Color image from the Kinect's camera with square showing head 
	tracked location, this location is relative	
4.A black image with any point where the player is believed to be
	colored gold, if no player it is just black
5.A black image with any point where the player is believed to be
	showing the player, if no player it is just black.
	Note will be off due to camera distance was never corrected
6.An image from  .\images\demo\ based on your current head tracked 
	position in degrees as well as by time if more than one frame
	is defined in the loadInstructions.txt, this image will be 
	the only image displayed if debuging is set to false. If player
	not found the image will stop moving, if the player has yet to
	be found since runtime it will be blank


User Manual
-------------------------------------------------------------------------------------------------------------------
				UI
Overview Screen (debug screen): If debug mode is set to true you will see this screen
	by default. You will see 5 images (6 if the kinect has at some point tracked you)
	and three debug values, degrees distance, degrees, and X. Where:
		degrees dist. is how far away you are on you current degree
		degrees is what radial the kinect is tacking your head at
		x is your distance left and right relative to the kinect
	You will also see a box with an error in it if any have happened. Clicking
	on an image will bring you to that images fullscreen mode	

Fullscreen: If debug mode is set to false you see image 6 in fullscreen by default. If toggled
	to true or set to true clicking on any screen that is fullsceen will send you back to
	the overview sceen. Whatever image you you clicked on in the overview screen is the
	image you see fullscreened. And all debug values are no long visable (however
	you will still see the error box on the side, I may change this overtime)

			Button & Mouse Clicks
There are a series of button presses that do the following:
Up Arrow - force the frame to change to the next frame
Down Arrow - force the frame to change to the previous frame
Left Arrow - lower the fps by one (stops at zero)
Right Arrow - raise the fps by one (No hard stop but unnoticable 
	once greater than refresh rate of screen)
Space Bar - it fps is not zero set the fps to zero (pause animation)
	if it is zero set the fps back to the fps in loadInstructions.txt
I Key - invert the order of images (image follow you or goes opposite way)
O Key - toggle allowing debug mode (showing all six images or just one)
	Still requires you to click on the image

Mouse Click - If an image is fullscreened and debug mode is true it will
	take the user back to the overview showing them all 6 images and
	the debug data
	Otherwise it will fullscreen the image you clicked on and hide debug
	the current debug data 

			loadInstructions.txt
The loadInstructions.txt contains loading instructions by default it would 
	look like this:

imageChangesPerDegree=1
numFrames=1
framesPerSecond=0
inversion=false
debugMode=false
kinectAngle=10

Side note each line it does not mater what is before the '=' the code only 
	interprets the string following it. An explanation of what each 
	line does goes as follows:
Line 1 formating: decimal=x.x (cant be zero)
	where x is the frequncy of images per degree so if you want to load 
	an image half a degree it would be .5, if you want to load it every 
	2 degrees it would be 2
	For best results you want your images to be across 60 degrees and
	then divide your number of images by 60. Then divide one by that number 
	That is the number you want to  put on line one 
	ie. I have 120 images across 60 degrees 1/(120/60) =.25 

	expert use: you can make the number slightly higher to make the
	images change faster or slightly lower to make the images change
	less often

Line 2 formating: integer=x (cant be zero)
	where x is the number of frames to be loaded so if I have 720 
	total images but each I have 12 sets of images that span across
	60 degrees each containing 60 images then the number I want is 12
	For best results you want your take your total number of images and
	divide it by the number of images that span across the 60 degree area
	ie. I have 720 image and over 60 degrees I have 120 images 720/120 =6

	dummy use: divide your number of image by 60 times your number on line one 

Line 3 formating: integer=x (can be zero, if previous line is 1 this line does not mater just put down a number)
	where x is the number of frames per second so if I want my animation to 
	change 12 times a second then the number is 12
	For best results use at least the number of frames you have if more than 24
	otherwise use at least 24, if your animation is to fast use a smaller number
	if it is to slow use a larger number. Left and right arrow can change this 
	during runtime.
	ie. I want to change my image at the same angle 30 times a second =30

	side note: This is independent of headtracking movement, that updates 30
	times a second and is hardcoded and can not be any faster on the current
	kinect

Line 4 formating: boolean=bool
	where bool is a boolean value (see what is boolean) this flips your order
	of images from left to right so if my images go from right to left then
	you want this value to be true. Hitting the I button during run time
	will toggle this vaule
	ie. my images are following me instead of going the other way, set it
	to the opposite value so if it was set to false set it to true

	Im not a computer scientist what is a boolean: true or false, 
	not t, not f, not 0, not 1, not 'true', not 'false', not flase (inside joke) 
	just true or false nothing more nothing less. You will get errors if you do 
	otherwise

Line 5 formating: boolean=bool
	where bool is a boolean value (see what is boolean) this allows for you to 
	view the debug mode (the overview screen with the 6 images) instead of just
	one. for more info look at the UI section Hitting the O button during run 
	time will turn this on
	ie. I dont want an image to be fullscreen by default or I want to see all 
	the other images/debug values so debug=true

	Im not a computer scientist what is a boolean: true or false, 
	not t, not f, not 0, not 1, not 'true', not 'false', not flase (inside joke) 
	just true or false nothing more nothing less. You will get errors if you do 
	otherwise

Line 6 formating: integer=x
	where x is an integer that defines what angle of the kinect. This is not
	relative of the kinect but relative of the angle flat to the ground so 90
	will always try to turn the kinect straight up
	ie. my kinect base is parallel to the ground and I want to looking 10 degrees
	more to the ground so angle =-10
line 7 formating: boolean=bool
	where bool is a boolean value that when true starts the program in paused mode
	when false starts the program unpaused
	ie. I have multiple frames of images and I want to start the program only 
	showing the first one so startPaused=true
	
	Im not a computer scientist what is a boolean: true or false, 
	not t, not f, not 0, not 1, not 'true', not 'false', not flase (inside joke) 
	just true or false nothing more nothing less. You will get errors if you do 
	otherwise
	
Line 8 formating: string=extension
	where extension is a type of file extension for images, the only ones supported
	are as follows:
	jpg/jpeg, bmp, gif, png, tiff

	ie. I have a series of bitmap or bmp images so fileType=bmp

Line 9 formating: string=speed
	where speed is if the program is to load in fast or slow mode, slow mode is only
	there for if you have to load those images and can not convert them to something 
	else but when loading in fast mode. If you put in something other than fast or slow
	the program assumes fast.
	ie. the program told me to use slow mode so loadMethod=slow

	tip: if you are using slow mode you may want to have a Solid State Drive or you
	will be droping frames

FAQ
-------------------------------------------------------------------------------------------------------------------
Why do I just have a full white screen? If debug mode is set to false in
	your loadInstructions you will see a white screen until it finds a
	player.
I dont have a kinect but I ran this with debug mode set to false how do I
	close it?
	Well hopefully you read this before that happened so you know if 
	you hit alt+f4 it will close it, or you could just alt+tab out 
	and close it on your taskbar normally.
I dont have a kinect for windows will this still work? No.
Will this work with the kinect from my Xbox? Short answer no. Long answer
	yes but I have not tested it, if microsoft is to be believed then 
	it will not work unless you run the source code in debug mode and 
	then you will liekly need to comment outthe following lines of 
	code which should be found around line 200:
		mySensor.SkeletonStream.EnableTrackingInNearRange = true; //enable near mode
                mySensor.DepthStream.Range = DepthRange.Near;
                mySensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated; //enable seated mode since we just care about the head
	this will disable near mode, which is something the Xbox kinect
	does not support. Then it should work.
Y U NO WORK? Did you read the readme?

Prerequists
-------------------------------------------------------------------------------------------------------------------
Download Kinect Runtime if you do not already have it or the sdk
http://go.microsoft.com/fwlink/?LinkID=275590&clcid=0x409

Contact Information
-------------------------------------------------------------------------------------------------------------------
This code and readme was writen by Trevor Hamliton during 2013 while
interning a the Office of High Performance of Computing and Communications,
at the National Library of Medicane, at the Lister Hill Center, in the 
National Institutes of Health, In the United States, on Earth.
This code is free to use and modify and is ditributed as is.

If you run into further issues email me at trevorhamilton93@gmail.com if
you do so you better have read every word in this .txt


tl;dr
-------------------------------------------------------------------------------------------------------------------
This is a readme you should read it if you have issues, otherwise good luck.