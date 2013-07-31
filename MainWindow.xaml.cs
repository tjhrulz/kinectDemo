using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.FaceTracking; //unused but this is for if you wanted to track face points
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace kinectDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        bool closing = false; //flag for if closing has been called

        const int blueIndex = 0; //const color bits for easier reading of code
        const int greenIndex = 1;
        const int redIndex = 2;

        BitmapSource[] images; //arrary of decoded images for display on image6
        string[] imagesName = new string[1]; //array of images locations for decoding
        int numImages = 0; //number of images found in demo directory

        int numFramesDropped = 0; //frame drop counter
        int imageFullscreened = 0; //which image is currently fullscreen (1-6)

        int imagesPerFrame = 0; //how many times the image will be split
        float splitSize = 0; // how many pixels across each split of the image is
        
        int numVars = 7; //number of virables read from file loadinstructions
        //default variables if file does not exist
        float imagesPerDegree = 1; //how frequently are images loaded (ie. every 1 degree, .5 degrees etc.)
        int numFrames = 1; //How many different sets of movement is there (ie. 12 frames would be 12 sets of images for different times each being the full amount of angles)
        int startingFramesPerSecond = 0; //Frames to change per second 0 is none, warning getting above refresh rate of monitor is pointless
        bool inversionBool = false; //invert order of images
        bool debugMode = false; //show overview
        int elevationAngle = 10; //angle of the kinect

        int timeLast = 0; //What time was it last time the function was called (format SS + MS ie. 25006)
        int framesPerSecond = 0; //current frame rate per second
        int currentFrame = 0; //current set of frames
        int inversion = 1;


        KinectSensor mySensor; //name of the kinect plugged in (Which is whatever one was plugged in first out of the current ones)
        //private readonly KinectSensorChooser myKinectChooser = new KinectSensorChooser();


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string imagesDirectory = System.IO.Directory.GetCurrentDirectory();
            if (Directory.Exists(imagesDirectory + "\\images\\demo\\"))
            {
                imagesDirectory = imagesDirectory + "\\images\\demo\\"; //go down to images\demo directory if it exists
            }

            numImages = Directory.GetFiles(imagesDirectory, "*.png", SearchOption.TopDirectoryOnly).Length; //count the number of .png files in the directory found above  
            imagesName = Directory.GetFiles(imagesDirectory, "*.png", SearchOption.TopDirectoryOnly); //every image path split into an array

            images = new BitmapSource[numImages];
            for (int i = 0; i < imagesName.Length; i++)
            {
                using (Stream imageStreamSource = new FileStream(imagesName[i], FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    PngBitmapDecoder decoder = new PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    images[i] = decoder.Frames[0]; //decode the images into a series of pixels and store them in images which is a BitmapSource
                }
            }
            try
            {
                using (StreamReader sr = new StreamReader("loadInstructions.txt"))
                {
                    bool isPaused = false;

                    String[] vars = new string[numVars];
                    for (int i = 0; i < numVars; i++)
                    {
                        vars[i] = sr.ReadLine();
                    }

                    //every line is expected to contain an = sign, read the string after that and convert as needed
                    try
                    {
                        imagesPerDegree = (float)Convert.ToDouble(vars[0].Substring(vars[0].LastIndexOf('=') + 1));
                    }
                    catch (Exception)
                    {
                        errorBox1.Text += " Load instructions misformatted on line 1"; //inform user of error but continue on
                        errorBox1.Visibility = System.Windows.Visibility.Visible;
                    }
                    try
                    {
                        numFrames = Convert.ToInt16(vars[1].Substring(vars[1].LastIndexOf('=') + 1));
                    }
                    catch (Exception)
                    {
                        errorBox1.Text += " Load instructions misformatted on line 2"; //inform user of error but continue on
                        errorBox1.Visibility = System.Windows.Visibility.Visible;
                    }
                    try
                    {
                        startingFramesPerSecond = Convert.ToInt16(vars[2].Substring(vars[2].LastIndexOf('=') + 1));
                    }
                    catch (Exception)
                    {
                        errorBox1.Text += " Load instructions misformatted on line 3"; //inform user of error but continue on
                        errorBox1.Visibility = System.Windows.Visibility.Visible;
                    }
                    try
                    {
                        inversionBool = Convert.ToBoolean(vars[3].Substring(vars[3].LastIndexOf('=') + 1));
                    }
                    catch (Exception)
                    {
                        errorBox1.Text += " Load instructions misformatted on line 4"; //inform user of error but continue on
                        errorBox1.Visibility = System.Windows.Visibility.Visible;
                    }
                    try
                    {
                        debugMode = Convert.ToBoolean(vars[4].Substring(vars[4].LastIndexOf('=') + 1));
                    }
                    catch (Exception)
                    {
                        errorBox1.Text += " Load instructions misformatted on line 5"; //inform user of error but continue on
                        errorBox1.Visibility = System.Windows.Visibility.Visible;
                    }
                    try
                    {
                        elevationAngle = Convert.ToInt16(vars[5].Substring(vars[5].LastIndexOf('=') + 1));
                    }
                    catch (Exception)
                    {
                        errorBox1.Text += " Load instructions misformatted on line 6"; //inform user of error but continue on
                        errorBox1.Visibility = System.Windows.Visibility.Visible;
                    }
                    try
                    {
                        isPaused = Convert.ToBoolean(vars[6].Substring(vars[6].LastIndexOf('=') + 1));
                    }
                    catch (Exception)
                    {
                        errorBox1.Text += " Load instructions misformatted on line 7"; //inform user of error but continue on
                        errorBox1.Visibility = System.Windows.Visibility.Visible;
                    }

                    if (inversionBool)
                    {
                        inversion = -inversion;
                    }
                    if (!debugMode)
                    {
                        image6_MouseDownManual();
                    }
                    if (isPaused)
                    {
                        framesPerSecond = 0;
                    }
                    else
                    {
                        framesPerSecond = startingFramesPerSecond;
                    }
                }
            }

            catch (IOException)
            {
                errorBox1.Text += " No load instructions found"; //inform user of error but continue on
                errorBox1.Visibility = System.Windows.Visibility.Visible;
            }
            catch (Exception)
            {
                errorBox1.Text += " Load instructions misformatted"; //inform user of error but continue on
                errorBox1.Visibility = System.Windows.Visibility.Visible;
            }


            float numSplits = numImages; //how many times the image will be split
            float splitSize = (float)image6.Width / numImages; // how many pixels across each split of the image is

            if (KinectSensor.KinectSensors.Count > 0)
            {
                mySensor = KinectSensor.KinectSensors[0]; //get the first kinect found

                if (mySensor.Status == KinectStatus.Connected)
                {
                    var parameters = new TransformSmoothParameters //turn all of the stuff off for speed
                    {
                        Smoothing = 0.0f, //higher is slower more smoothing default=.5
                        Correction = 0.99f, //high is faster but "less smooth" and corrects to raw data faster"? default=.5
                        Prediction = 0.0f, //nobody knows anything but "Specifies the number of predicted frames" default=.5
                        JitterRadius = 0.0f, //jitter radius in meters so .05 will return a change under 5cm to be the same default=.05
                        MaxDeviationRadius = 0.0f //max amount in meters that a value after being "filtered can be from its original value default=.04
                    };
                    //On window load once a kinect is found enable all needed streams
                    mySensor.ColorStream.Enable();
                    mySensor.DepthStream.Enable();
                    //mySensor.SkeletonStream.Enable();
                    mySensor.SkeletonStream.Enable(parameters);

                    mySensor.SkeletonStream.EnableTrackingInNearRange = true; //enable near mode
                    mySensor.DepthStream.Range = DepthRange.Near;
                    mySensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated; //enable seated mode since we just care about the head

                    mySensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(mySensor_AllFramesReady);
                    try
                    {
                        mySensor.Start(); //Try to turn it on 
                    }
                    catch (System.IO.IOException)
                    {
                        throw; //Catching exception for a program is already using it
                    }
                    mySensor.ElevationAngle = elevationAngle; // change elevation angle to whatever hieght I want it
                }
            }
            //myKinectChooser.KinectChanged +=new EventHandler<KinectChangedEventArgs>(myKinectChooser_KinectChanged);
        }

        void myKinectChooser_KinectChanged(object sender, KinectChangedEventArgs e)
        {
            //Old code that used predefined stuff I could not control but I kept it around
            /*KinectSensor oldSensor = (KinectSensor)e.OldSensor;
            stopKinect(oldSensor);

            KinectSensor newSensor = (KinectSensor)e.NewSensor;
            newSensor.ColorStream.Enable();
            newSensor.DepthStream.Enable();
            newSensor.SkeletonStream.Enable();
            newSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(mySensor_AllFramesReady);
            try
            {
                newSensor.Start();
            }
            catch (System.IO.IOException)
            {
                throw;
            }*/
        }

        void mySensor_AllFramesReady(object sender, AllFramesReadyEventArgs e) //when every frame I have enabled is ready
        {
            if (closing) //dont do anything if program is closing
            {
                return;
            }

            textBlock1.Text = "Number of Frames Droped Total: " + numFramesDropped.ToString();


            //if on overview menu laod the all or if that perticular image is fullscreened load that one
            if (imageFullscreened == 0 || imageFullscreened == 1)
            {
                //display current color image frame
                using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                {
                    if (colorFrame == null) //if no data because dropped frame
                    {
                        numFramesDropped++;
                        return;
                    }

                    byte[] pixels = new byte[colorFrame.PixelDataLength]; //An array of bytes to hold colors
                    colorFrame.CopyPixelDataTo(pixels); //copy the color image to that array

                    int stride = colorFrame.Width * 4; //How many pixels per row
                    image1.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride); //display work on image 1
                }
            }
            if (imageFullscreened == 0 || imageFullscreened == 2)
            {
                //display depth data based on color
                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    if (depthFrame == null) //if no data because dropped frame
                    {
                        numFramesDropped++;
                        return;
                    }

                    byte[] pixels = generateColoredBytes(depthFrame); //generate an array of bytes in bgr format

                    int stride = depthFrame.Width * 4;//How many pixels per row
                    image2.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride); //display work on image 2
                }
            }
            if (imageFullscreened == 0 || imageFullscreened == 3 || imageFullscreened == 4)
            {
                //color the 3rd image with the player gold on a black background and the 4th image make everything but the player black
                using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    if (colorFrame == null || depthFrame == null) //if no data because dropped frame
                    {
                        numFramesDropped++;
                        return;
                    }

                    byte[] pixels = new byte[colorFrame.PixelDataLength]; //An array of bytes to hold colors 
                    //colorFrame.CopyPixelDataTo(pixels); //copy from color image to array

                    if (imageFullscreened == 0 || imageFullscreened == 3)
                    {
                        pixels = generatePlayerBytes(depthFrame, pixels); //Color over that array with gold on any pixel with a player flag true (up to 6 players)
                        int stride = colorFrame.Width * 4;//How many pixels per row
                        image3.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride); //display work on image 3
                    }
                    if (imageFullscreened == 0 || imageFullscreened == 4)
                    {
                        colorFrame.CopyPixelDataTo(pixels); //reset work
                        byte[] playerPixels = generateNotPlayerBytes(depthFrame, pixels); //Image is black anywhere there is not believed to be a player
                        int stride = colorFrame.Width * 4;//How many pixels per row
                        image4.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride); //display work on image 4
                    }
                }
            }
            if (imageFullscreened == 0 || imageFullscreened == 5)
            {
                //show color data with a square drawn relative to the players head (not absolute)
                using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (colorFrame == null || skeletonFrame == null)
                    {
                        numFramesDropped++;
                        return;
                    }

                    byte[] pixels = new byte[colorFrame.PixelDataLength]; //An array of bytes to hold colors 
                    colorFrame.CopyPixelDataTo(pixels); //copy from color image to array

                    const int skeletonCount = 6;
                    Skeleton[] allSkeletons = new Skeleton[skeletonCount];
                    skeletonFrame.CopySkeletonDataTo(allSkeletons);

                    Skeleton firstPlayer = (from s in allSkeletons where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();

                    int stride = colorFrame.Width * 4; //How many pixels per row
                    if (firstPlayer == null)
                    {
                        image5.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride); //display partial work on image 5
                        return;
                    }

                    float headDepthPointX = firstPlayer.Joints[JointType.Head].Position.X;
                    float headDepthPointY = firstPlayer.Joints[JointType.Head].Position.Y;
                    float headDepthPointZ = firstPlayer.Joints[JointType.Head].Position.Z;

                    headDepthPointX = Scale(colorFrame.Width, 1, headDepthPointX);
                    headDepthPointY = Scale(colorFrame.Width, 1, -headDepthPointY);

                    for (int locationY = 0, colorIndex = 0; locationY < colorFrame.Height; locationY++)
                    {
                        for (int locationX = 0; locationX < colorFrame.Width; locationX++, colorIndex += 4)
                        {
                            if (locationX % colorFrame.Width >= (headDepthPointX) - 15 && locationX % colorFrame.Width <= headDepthPointX + 15 && locationY % colorFrame.Height >= headDepthPointY - 15 && locationY % colorFrame.Height <= headDepthPointY + 15)
                            {
                                pixels[colorIndex + blueIndex] = 0;
                                pixels[colorIndex + greenIndex] = 215;
                                pixels[colorIndex + redIndex] = 255;
                            }
                        }
                    }
                    image5.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride); //display work on image 5
                }
            }

            if (imageFullscreened == 0 || imageFullscreened == 6)
            {
                //display image from \images\demo\ based on head location and loadInstructions.txt
                using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (skeletonFrame == null || colorFrame == null)
                    {
                        numFramesDropped++;
                        return;
                    }
                    const int skeletonCount = 6;
                    Skeleton[] allSkeletons = new Skeleton[skeletonCount];
                    skeletonFrame.CopySkeletonDataTo(allSkeletons);

                    Skeleton firstPlayer = (from s in allSkeletons where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();

                    int stride = colorFrame.Width * 4; //How many pixels per row
                    if (firstPlayer == null)
                    {
                        //image6.Source = null;
                        //image6.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride); //display partial work on image 6
                        return;
                    }

                    //motionPicture.Size = this.Size;

                    if (numImages != 0)
                    {
                        //basic declarations
                        imagesPerFrame = numImages / numFrames; //how many times the image will be split
                        splitSize = (float)image6.Width / imagesPerFrame; // how many pixels across each split of the image is

                        float degreesDist = (float)Math.Pow(firstPlayer.Joints[JointType.Head].Position.Z, 2) + (float)Math.Pow(firstPlayer.Joints[JointType.Head].Position.X, 2);

                        //double degrees = Math.Asin((firstPlayer.Joints[JointType.Head].Position.X / degreesDist)) * (180 / Math.PI); //since degreesDist was not given I decided not to use
                        double degrees = Math.Atan(firstPlayer.Joints[JointType.Head].Position.X / firstPlayer.Joints[JointType.Head].Position.Z) * (180 / Math.PI); //get degrees from X and Y locations 

                        int imageToLoad = (int)ImageFromDegrees(inversion * degrees, imagesPerDegree, imagesPerFrame); //send info to image to load (degrees negative if angle needs to be flipped)

                        if (numFrames > 1)
                        {
                            //do time load
                            imageToLoad = (int)ImageFromTime(imageToLoad, imagesPerFrame, numFrames, framesPerSecond, ref timeLast, ref currentFrame);  //if more than one set of frames get which set to load based of FPS
                        }

                        image6.Source = images[imageToLoad];//load apropriate image array into the image source

                        //update debug data (move later)
                        debugBlock1.Text = "Degrees Dist: " + degreesDist;
                        debugBlock2.Text = "Degrees: " + degrees;
                        debugBlock3.Text = "X: " + firstPlayer.Joints[JointType.Head].Position.X;
                    }
                }
            }
        }

        void stopKinect(KinectSensor sensor) //call to stop the kinect
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.AudioSource.Stop();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) //catch window close and clean everything up
        {
            closing = true;
            stopKinect(mySensor);
        }
        private byte[] generatePlayerBytes(DepthImageFrame depthFrame, byte[] pixels) //generate where the player is and color them gold the return pixel array
        {
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            //Byte[] playerPixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            //const int blueIndex = 0;
            //const int greenIndex = 1;
            //const int redIndex = 2;

            for (int depthIndex = 0, colorIndex = 0; depthIndex < rawDepthData.Length && colorIndex < pixels.Length; depthIndex++, colorIndex += 4)
            {
                int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;

                //int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if (player > 0)
                {
                    pixels[colorIndex + blueIndex] = 0;
                    pixels[colorIndex + greenIndex] = 215;
                    pixels[colorIndex + redIndex] = 255;
                }
            }

            return pixels;
        }

        private byte[] generateNotPlayerBytes(DepthImageFrame depthFrame, byte[] pixels) //generate where the player is not and color it black  
        {
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            //Byte[] playerPixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            //const int blueIndex = 0;
            //const int greenIndex = 1;
            //const int redIndex = 2;

            for (int depthIndex = 0, colorIndex = 0; depthIndex < rawDepthData.Length && colorIndex < pixels.Length; depthIndex++, colorIndex += 4)
            {
                int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;

                //int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if (player == 0)
                {
                    pixels[colorIndex + blueIndex] = 0;
                    pixels[colorIndex + greenIndex] = 0;
                    pixels[colorIndex + redIndex] = 0;
                }
            }

            return pixels;
        }

        private byte[] generateColoredBytes(DepthImageFrame depthFrame) //color bytes based on depth
        {
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            Byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            //const int blueIndex = 0;
            //const int greenIndex = 1;
            //const int redIndex = 2;

            for (int depthIndex = 0, colorIndex = 0; depthIndex < rawDepthData.Length && colorIndex < pixels.Length; depthIndex++, colorIndex += 4)
            {
                int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;

                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                //byte bgrColor = calculateIntensityDepth(depth);


                if (depth > 400 && depth <= 1000)
                {
                    pixels[colorIndex + blueIndex] = 255;
                    pixels[colorIndex + greenIndex] = 0;
                    pixels[colorIndex + redIndex] = 0;
                }
                else if (depth > 1000 && depth <= 3000)
                {
                    pixels[colorIndex + blueIndex] = 0;
                    pixels[colorIndex + greenIndex] = 255;
                    pixels[colorIndex + redIndex] = 0;
                }
                else if (depth > 3000 && depth <= 4000)
                {
                    pixels[colorIndex + blueIndex] = 0;
                    pixels[colorIndex + greenIndex] = 0;
                    pixels[colorIndex + redIndex] = 255;
                }
                else
                {
                    pixels[colorIndex + blueIndex] = 255;
                    pixels[colorIndex + greenIndex] = 255;
                    pixels[colorIndex + redIndex] = 255;
                }
                /*if (player > 0)
                {
                    pixels[colorIndex + blueIndex] = 0;
                    pixels[colorIndex + greenIndex] = 215;
                    pixels[colorIndex + redIndex] = 255;
                }*/

                //pixels[colorIndex + blueIndex] = bgrColor;
                //pixels[colorIndex + greenIndex] = bgrColor;
                //pixels[colorIndex + redIndex] = bgrColor;

            }
            return pixels;
        }

        private byte calculateIntensityDepth(int depth) //unused similar to generateColoredBytes
        {
            byte calculatedColor = (byte)(255 - (255 * Math.Max(depth - 900, 0) / 2000));

            return calculatedColor;
        }

        private static float Scale(float maxPixel, float maxSkeleton, float position) //scale position for display on an image with min and max the size of the image, relative not absolute
        {
            float value = ((((maxPixel / maxSkeleton) / 2) * position) + (maxPixel / 2)); //convert to a pixel mapping with min maxs
            if (value > maxPixel)
                return maxPixel - 1;
            if (value < 0)
                return 0;
            return value;
        }
        public static float ImageFromDegrees(double degrees, double degreesPerImage, int numImages) //similar to above but based of degrees protected from crashes and scaleable 
        {
            float loadImage = (int)(numImages / 2) + (int)(degrees / degreesPerImage); //same as aboce but with degrees in mind to start
            if (loadImage >= numImages)
                return numImages - 1;
            if (loadImage < 0)
                return 0;
            return loadImage;
        }
        public static float ImageFromTime(int imageToLoad, int imagesPerFrame, int numFrames, int framesPerSecond, ref int lastTime , ref int currentFrame) //based on which image was decided before figure out which frame to load
        {
            if (framesPerSecond != 0) //if more than one frame exists
            {
                int currentTime = DateTime.Now.Second * 1000 + DateTime.Now.Millisecond; //seconds extanious but needed for testing and for 1 fps
                double fps = 1000 / framesPerSecond; //figure out fps in ms

                if (currentTime < lastTime) //if current time is smaller than the last time 
                {
                    currentTime = currentTime + 60000; //add a minute as a minute wrap around had to happen
                }
                if (currentTime >= lastTime + fps) //if the current time is at or greater than the time to update
                {
                    //figure out which frame to load
                    if (currentFrame == numFrames - 1)
                    {
                        //start over if end was reached
                        currentFrame = 0;
                    }
                    else
                    {
                        currentFrame++;
                    }
                    if (currentTime > 60000)
                    {
                        //if a minute was added undo it
                        currentTime = currentTime - 60000;
                    }
                    lastTime = currentTime; //update time
                }
            }
            return imageToLoad + (currentFrame * imagesPerFrame);
        }

        private void image6_MouseDownManual()
        {
            debugBlock1.Visibility = System.Windows.Visibility.Hidden;
            debugBlock2.Visibility = System.Windows.Visibility.Hidden;
            debugBlock3.Visibility = System.Windows.Visibility.Hidden;
            textBlock1.Visibility = System.Windows.Visibility.Hidden;

            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;

            image1.Visibility = System.Windows.Visibility.Hidden;
            image2.Visibility = System.Windows.Visibility.Hidden;
            image3.Visibility = System.Windows.Visibility.Hidden;
            image4.Visibility = System.Windows.Visibility.Hidden;
            image5.Visibility = System.Windows.Visibility.Hidden;
            image6.Visibility = System.Windows.Visibility.Visible;

            image6.Height = this.ActualHeight;
            image6.Width = this.ActualWidth;

            image6.Margin = new Thickness(0, 0, 0, 0);

            imageFullscreened = 6;
        }


        private void image1_MouseDown(object sender, MouseButtonEventArgs e) //if you clicked on first image toggle fullscreen
        {
            if (imageFullscreened == 0)
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;

                debugBlock1.Visibility = System.Windows.Visibility.Hidden;
                debugBlock2.Visibility = System.Windows.Visibility.Hidden;
                debugBlock3.Visibility = System.Windows.Visibility.Hidden;
                textBlock1.Visibility = System.Windows.Visibility.Hidden;

                image1.Visibility = System.Windows.Visibility.Visible;
                image2.Visibility = System.Windows.Visibility.Hidden;
                image3.Visibility = System.Windows.Visibility.Hidden;
                image4.Visibility = System.Windows.Visibility.Hidden;
                image5.Visibility = System.Windows.Visibility.Hidden;
                image6.Visibility = System.Windows.Visibility.Hidden;

                image1.Height = this.ActualHeight;
                image1.Width = this.ActualWidth;

                imageFullscreened = 1;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;

                debugBlock1.Visibility = System.Windows.Visibility.Visible;
                debugBlock2.Visibility = System.Windows.Visibility.Visible;
                debugBlock3.Visibility = System.Windows.Visibility.Visible;
                textBlock1.Visibility = System.Windows.Visibility.Visible;

                image1.Visibility = System.Windows.Visibility.Visible;
                image2.Visibility = System.Windows.Visibility.Visible;
                image3.Visibility = System.Windows.Visibility.Visible;
                image4.Visibility = System.Windows.Visibility.Visible;
                image5.Visibility = System.Windows.Visibility.Visible;
                image6.Visibility = System.Windows.Visibility.Visible;

                image1.Height = 240;
                image1.Width = 320;

                imageFullscreened = 0;
            }
        }

        private void image2_MouseDown(object sender, MouseButtonEventArgs e) //if you clicked on second image toggle fullscreen
        {
            if (imageFullscreened == 0)
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;

                debugBlock1.Visibility = System.Windows.Visibility.Hidden;
                debugBlock2.Visibility = System.Windows.Visibility.Hidden;
                debugBlock3.Visibility = System.Windows.Visibility.Hidden;
                textBlock1.Visibility = System.Windows.Visibility.Hidden;

                image1.Visibility = System.Windows.Visibility.Hidden;
                image2.Visibility = System.Windows.Visibility.Visible;
                image3.Visibility = System.Windows.Visibility.Hidden;
                image4.Visibility = System.Windows.Visibility.Hidden;
                image5.Visibility = System.Windows.Visibility.Hidden;
                image6.Visibility = System.Windows.Visibility.Hidden;

                image2.Height = this.ActualHeight;
                image2.Width = this.ActualWidth;

                Canvas.SetTop(image2, 0);
                Canvas.SetLeft(image2, 0);

                image2.Margin = new Thickness(0, 0, 0, 0);

                imageFullscreened = 2;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;

                debugBlock1.Visibility = System.Windows.Visibility.Visible;
                debugBlock2.Visibility = System.Windows.Visibility.Visible;
                debugBlock3.Visibility = System.Windows.Visibility.Visible;
                textBlock1.Visibility = System.Windows.Visibility.Visible;

                image1.Visibility = System.Windows.Visibility.Visible;
                image2.Visibility = System.Windows.Visibility.Visible;
                image3.Visibility = System.Windows.Visibility.Visible;
                image4.Visibility = System.Windows.Visibility.Visible;
                image5.Visibility = System.Windows.Visibility.Visible;
                image6.Visibility = System.Windows.Visibility.Visible;

                image2.Height = 240;
                image2.Width = 320;

                image2.Margin = new Thickness(318, 0, 0, 0);

                imageFullscreened = 0;
            }
        }

        private void image3_MouseDown(object sender, MouseButtonEventArgs e) //if you clicked on third image toggle fullscreen
        {
            if (imageFullscreened == 0)
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;

                debugBlock1.Visibility = System.Windows.Visibility.Hidden;
                debugBlock2.Visibility = System.Windows.Visibility.Hidden;
                debugBlock3.Visibility = System.Windows.Visibility.Hidden;
                textBlock1.Visibility = System.Windows.Visibility.Hidden;

                image1.Visibility = System.Windows.Visibility.Hidden;
                image2.Visibility = System.Windows.Visibility.Hidden;
                image3.Visibility = System.Windows.Visibility.Visible;
                image4.Visibility = System.Windows.Visibility.Hidden;
                image5.Visibility = System.Windows.Visibility.Hidden;
                image6.Visibility = System.Windows.Visibility.Hidden;

                image3.Height = this.ActualHeight;
                image3.Width = this.ActualWidth;

                image3.Margin = new Thickness(0, 0, 0, 0);

                imageFullscreened = 3;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;

                debugBlock1.Visibility = System.Windows.Visibility.Visible;
                debugBlock2.Visibility = System.Windows.Visibility.Visible;
                debugBlock3.Visibility = System.Windows.Visibility.Visible;
                textBlock1.Visibility = System.Windows.Visibility.Visible;

                image1.Visibility = System.Windows.Visibility.Visible;
                image2.Visibility = System.Windows.Visibility.Visible;
                image3.Visibility = System.Windows.Visibility.Visible;
                image4.Visibility = System.Windows.Visibility.Visible;
                image5.Visibility = System.Windows.Visibility.Visible;
                image6.Visibility = System.Windows.Visibility.Visible;

                image3.Height = 240;
                image3.Width = 320;

                image3.Margin = new Thickness(0, 240, 0, 0);

                imageFullscreened = 0;
            }
        }

        private void image4_MouseDown(object sender, MouseButtonEventArgs e) //if you clicked on fourth image toggle fullscreen
        {
            if (imageFullscreened == 0)
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;

                debugBlock1.Visibility = System.Windows.Visibility.Hidden;
                debugBlock2.Visibility = System.Windows.Visibility.Hidden;
                debugBlock3.Visibility = System.Windows.Visibility.Hidden;
                textBlock1.Visibility = System.Windows.Visibility.Hidden;

                image1.Visibility = System.Windows.Visibility.Hidden;
                image2.Visibility = System.Windows.Visibility.Hidden;
                image3.Visibility = System.Windows.Visibility.Hidden;
                image4.Visibility = System.Windows.Visibility.Visible;
                image5.Visibility = System.Windows.Visibility.Hidden;
                image6.Visibility = System.Windows.Visibility.Hidden;

                image4.Height = this.ActualHeight;
                image4.Width = this.ActualWidth;

                image4.Margin = new Thickness(0, 0, 0, 0);

                imageFullscreened = 4;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;

                debugBlock1.Visibility = System.Windows.Visibility.Visible;
                debugBlock2.Visibility = System.Windows.Visibility.Visible;
                debugBlock3.Visibility = System.Windows.Visibility.Visible;
                textBlock1.Visibility = System.Windows.Visibility.Visible;

                image1.Visibility = System.Windows.Visibility.Visible;
                image2.Visibility = System.Windows.Visibility.Visible;
                image3.Visibility = System.Windows.Visibility.Visible;
                image4.Visibility = System.Windows.Visibility.Visible;
                image5.Visibility = System.Windows.Visibility.Visible;
                image6.Visibility = System.Windows.Visibility.Visible;

                image4.Height = 240;
                image4.Width = 320;

                image4.Margin = new Thickness(318, 240, 0, 0);

                imageFullscreened = 0;
            }
        }

        private void image5_MouseDown(object sender, MouseButtonEventArgs e) //if you clicked on fifth image toggle fullscreen
        {
            if (imageFullscreened == 0)
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;

                debugBlock1.Visibility = System.Windows.Visibility.Hidden;
                debugBlock2.Visibility = System.Windows.Visibility.Hidden;
                debugBlock3.Visibility = System.Windows.Visibility.Hidden;
                textBlock1.Visibility = System.Windows.Visibility.Hidden;

                image1.Visibility = System.Windows.Visibility.Hidden;
                image2.Visibility = System.Windows.Visibility.Hidden;
                image3.Visibility = System.Windows.Visibility.Hidden;
                image4.Visibility = System.Windows.Visibility.Hidden;
                image5.Visibility = System.Windows.Visibility.Visible;
                image6.Visibility = System.Windows.Visibility.Hidden;

                image5.Height = this.ActualHeight;
                image5.Width = this.ActualWidth;

                image5.Margin = new Thickness(0, 0, 0, 0);

                imageFullscreened = 5;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;

                debugBlock1.Visibility = System.Windows.Visibility.Visible;
                debugBlock2.Visibility = System.Windows.Visibility.Visible;
                debugBlock3.Visibility = System.Windows.Visibility.Visible;
                textBlock1.Visibility = System.Windows.Visibility.Visible;

                image1.Visibility = System.Windows.Visibility.Visible;
                image2.Visibility = System.Windows.Visibility.Visible;
                image3.Visibility = System.Windows.Visibility.Visible;
                image4.Visibility = System.Windows.Visibility.Visible;
                image5.Visibility = System.Windows.Visibility.Visible;
                image6.Visibility = System.Windows.Visibility.Visible;

                image5.Height = 240;
                image5.Width = 320;

                image5.Margin = new Thickness(638, 0, 0, 0);

                imageFullscreened = 0;
            }
        }

        private void image6_MouseDown(object sender, MouseButtonEventArgs e) //if you clicked on sixth image toggle fullscreen
        {
            if (imageFullscreened == 0)
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;

                debugBlock1.Visibility = System.Windows.Visibility.Hidden;
                debugBlock2.Visibility = System.Windows.Visibility.Hidden;
                debugBlock3.Visibility = System.Windows.Visibility.Hidden;
                textBlock1.Visibility = System.Windows.Visibility.Hidden;

                image1.Visibility = System.Windows.Visibility.Hidden;
                image2.Visibility = System.Windows.Visibility.Hidden;
                image3.Visibility = System.Windows.Visibility.Hidden;
                image4.Visibility = System.Windows.Visibility.Hidden;
                image5.Visibility = System.Windows.Visibility.Hidden;
                image6.Visibility = System.Windows.Visibility.Visible;

                image6.Height = this.ActualHeight;
                image6.Width = this.ActualWidth;

                image6.Margin = new Thickness(0, 0, 0, 0);

                imageFullscreened = 6;
            }
            else if (debugMode)
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;

                debugBlock1.Visibility = System.Windows.Visibility.Visible;
                debugBlock2.Visibility = System.Windows.Visibility.Visible;
                debugBlock3.Visibility = System.Windows.Visibility.Visible;
                textBlock1.Visibility = System.Windows.Visibility.Visible;

                image1.Visibility = System.Windows.Visibility.Visible;
                image2.Visibility = System.Windows.Visibility.Visible;
                image3.Visibility = System.Windows.Visibility.Visible;
                image4.Visibility = System.Windows.Visibility.Visible;
                image5.Visibility = System.Windows.Visibility.Visible;
                image6.Visibility = System.Windows.Visibility.Visible;

                image6.Height = 240;
                image6.Width = 320;

                image6.Margin = new Thickness(638, 240, 0, 0);

                imageFullscreened = 0;
            }
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Left) //if left arrow pressed turn down fps
            {
                if (framesPerSecond != 0)
                {
                    framesPerSecond--;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Right) //if right arrow pressed turn up fps
            {
                framesPerSecond++;
                e.Handled = true;
            }
            else if (e.Key == Key.Space) //if space pressed toggle pause/play (also works as a reset button)
            {
                if (framesPerSecond != 0)
                {
                    framesPerSecond = 0;
                }
                else
                {
                    framesPerSecond = startingFramesPerSecond;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Up) //If up arrow pressed manually change frame forward
            {
                if (currentFrame == numFrames - 1)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame++;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Down) //If down arrow pressed manually change frame backward
            {
                if (currentFrame == 0)
                {
                    currentFrame = numFrames - 1;
                }
                else
                {
                    currentFrame--;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.I) //Toggle inversion (unimplmented)
            {
                inversion = -inversion;
                e.Handled = true;
            }
            else if (e.Key == Key.O) //If down arrow pressed manually change frame backward
            {
                debugMode = !debugMode;
                e.Handled = true;
            }
        }
    }
}
