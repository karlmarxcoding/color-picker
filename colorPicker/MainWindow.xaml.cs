using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace colorPicker
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);
         //----
         //[DllImport("user32.dll")] 
         // static extern int GetSystemMetrics(int nIndex);

        // [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        //  public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop); 
        //---- first implementation of color picker

        bool picking = false;
        System.Windows.Media.Color color;

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer animTimer = new System.Windows.Threading.DispatcherTimer();


        public MainWindow()
        {
            InitializeComponent();
            Cursor cur = new Cursor(new System.IO.MemoryStream(colorPicker.Properties.Resources.Cross));
            picker.Cursor = cur;
            
            string[] daargs = Environment.GetCommandLineArgs();
            Console.WriteLine("args {0}", daargs);
            if (1 < daargs.Length)
            {
                if (daargs[1] == "debug") { MessageBox.Show("BRUH DEBUGGING METHOD LOADED"); MessageBox.Show("THIS WONT DO SHIT BECAUSE DEV IS LAZY"); }
                
            }
        }

        private void colorPickStartButtonClick(object sender, RoutedEventArgs e)//this is the button that start the pick
        {
            dispatcherTimer.Tick += colorPick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1);
            dispatcherTimer.Start(); //this is a timer that runs function that checks the color and updates the preview

            Mouse.Capture(this, CaptureMode.SubTree); //this enables getting mouse events outside of the form window thing
            outForm();//this is what actaullly gets mouse events and sets fuction
            
        }

     

        private void colorPick(object sender, EventArgs e)
        {
       
            picking = true;
            
                System.Drawing.Point cursor = new System.Drawing.Point();
                GetCursorPos(ref cursor);
                picking = true;
                var c = GetColorAt(cursor); //this calls function that checks what color pixel is under the cursor
            
                color = System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
            setColor(color);//function that updated rgb and hex values
         

           // pic.Source = sta(cursor);

            if (picking)
            {
                System.Windows.Media.Color hex = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF51E099"); //color change on pick start button
                picker.Background = new SolidColorBrush(hex);
                picker.BorderBrush = new SolidColorBrush(hex);

            }
           



        }

        private void setColor(System.Windows.Media.Color cs)
        {
            string hex = new System.Windows.Media.ColorConverter().ConvertToString(cs); //this handles the colors and converts is
            colorView.Fill = new SolidColorBrush(cs);
            hexBox.Text = "#" + cs.R.ToString("X2") + cs.G.ToString("X2") + cs.B.ToString("X2");
            redR.Text = cs.R.ToString();
            greenG.Text = cs.G.ToString();
            blueB.Text = cs.B.ToString();


        }

        public System.Drawing.Color GetColorAt(System.Drawing.Point location) //dont know why this is public 
        {
            using (var bitmap = new Bitmap(1, 1))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(location, new System.Drawing.Point(0, 0), new System.Drawing.Size(1, 1));
                }
                return bitmap.GetPixel(0, 0); //best imp really well coded by smart me :) //working an avg pick
            }
        }


        private void outForm()
        {
            AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(stopPick), true); //sets stopPick func as subtree mouse event (outside of the window)
        }

        private void stopPick(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            dispatcherTimer.Stop(); //stops update of func
            storeColor(color);
            picking = false;


        }

        private void storeColor(System.Windows.Media.Color c)
        {
            System.Windows.Media.Color ccc = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFC6C4D");
            picker.Background = new SolidColorBrush(ccc); picker.BorderBrush = new SolidColorBrush(ccc);

            foreach (System.Windows.Shapes.Rectangle r in storedC.Children)
            {

                string hex = new System.Windows.Media.ColorConverter().ConvertToString(r.Fill);
                string hex2 = new System.Windows.Media.ColorConverter().ConvertToString(c);

                if (hex == "#FF535557") //this is great, basically it check if the store box is colored or not 
                {
                    r.Fill = new SolidColorBrush(c);
                    break;

                } else if (hex2 == hex) {

                    break;

                } else {
                   
                }


            }

        }

        private void DragControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            foreach (System.Windows.Shapes.Rectangle r in storedC.Children)
            {
                System.Windows.Media.Color hex = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF535557");
                r.Fill = new SolidColorBrush(hex);
            }

        } //this is the gratest piece of code ive ever written



        private void getColorFromStore(object sender, MouseButtonEventArgs e)
        {
            copyStr.Visibility = Visibility.Hidden;
            System.Windows.Shapes.Rectangle rct = (System.Windows.Shapes.Rectangle)sender;
            System.Windows.Media.Brush cs = rct.Fill;
            System.Windows.Media.Color c = ((SolidColorBrush)cs).Color; 

            string hex = new System.Windows.Media.ColorConverter().ConvertToString(rct.Fill);
            var hey = "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
            hexBox.Text = hey;

            redR.Text = c.R.ToString();
            greenG.Text = c.G.ToString();
            blueB.Text = c.B.ToString();
            colorView.Fill = cs;
            copyStr.Visibility = Visibility.Visible;

            animTimer.Tick += (s, ee) => { copyStr.Visibility = Visibility.Hidden; animTimer.Stop(); };
            animTimer.Interval = TimeSpan.FromSeconds(2);
            animTimer.Start();

            Clipboard.SetText(hey);
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e) //gets color inside window
        {
            if (picking)
            {
                picking = false;

                dispatcherTimer.Stop();
                storeColor(color);
            }
        }

        private void updateColorViewOnEnter(object sender, KeyEventArgs e)
        {

            if (e.Key < Key.D0 || e.Key > Key.D9) //ONLY NUMERIC implementation blessed by Allah
            {
                e.Handled = true;
            }

            if (e.Key == Key.Enter)


                if (redR.Text == "" || greenG.Text == "" || blueB.Text == "")
                {
                    animationHandle("an rgb value is empty!");
                    return;
                }
                else
                {
                    
                        Byte r = Convert.ToByte(redR.Text);
                        Byte g = Convert.ToByte(greenG.Text);
                        Byte b = Convert.ToByte(blueB.Text);
                        System.Windows.Media.Color color2 = System.Windows.Media.Color.FromRgb(r, g, b);
                        setColor(color2);
                  
                }
        }


        private void updateColorHex(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)

                if (hexBox.Text != "")
                {
                    if (Regex.IsMatch(hexBox.Text, @"^#([a-fA-F0-9]{6})$")) //regex made by Allah itself
                    {

                        System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexBox.Text);
                        setColor(color);
                    } else
                    { animationHandle("hex value is invalid!"); 
                        return;  }
                }
                else {
                    animationHandle("hex value is empty!");
                    return; 
                }
        }

        /*
        private BitmapImage sta(System.Drawing.Point location)
        {
             using (var bitmap = new Bitmap(80, 80))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {

                    graphics.CopyFromScreen(location.X - 40, location.Y - 40, 0, 0, new System.Drawing.Size(80, 80));
                    //  graphics.DrawLine(new System.Drawing.Pen(System.Drawing.Brushes.White, 1), new System.Drawing.Point(0, 0), new System.Drawing.Point(40, 0));
                    //  graphics.DrawLine(new System.Drawing.Pen(System.Drawing.Brushes.White, 1), new System.Drawing.Point(0, 0), new System.Drawing.Point(0, 40));
                    //  graphics.DrawLine(new System.Drawing.Pen(System.Drawing.Brushes.White, 1), new System.Drawing.Point(0, 0), new System.Drawing.Point(40, 40));
                    for (int x = 40; x < 40; x++)
                    {
                        for (int y = 100; y < 120; y++)
                        {
                            bitmap.SetPixel(x, y, System.Drawing.Color.Blue);
                        }
                    }
                    using (MemoryStream memory = new MemoryStream())
                    {
                        
                        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                        memory.Position = 0;
                        BitmapImage bitmapimage = new BitmapImage();
                        bitmapimage.BeginInit();
                        bitmapimage.StreamSource = memory;
                        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapimage.EndInit();

                        return bitmapimage;
                    }
                }
            }
        }
        */ //shit implementation new implementation is good

        private void closeButton(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void minimizeButton(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();

        }

        private void animationHandle(string text)
        {
            copyStr.Content = text;
            copyStr.Visibility = Visibility.Hidden;

            copyStr.Visibility = Visibility.Visible;
            animTimer.Tick += (s, ee) => { copyStr.Visibility = Visibility.Hidden; animTimer.Stop(); };
            animTimer.Interval = TimeSpan.FromSeconds(4);
            animTimer.Start();
        }

        private void helpButton(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("click on textbox then use ctrl + a to select all the text and ctrl + c to copy", "useful help very useful", MessageBoxButton.OK);
        }
    }










}
