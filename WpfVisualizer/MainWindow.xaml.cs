using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;
using Petzold.Media3D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace WpfVisualizer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var group = ViewPort.RenderTransform as TransformGroup;
            foreach (var transform in group.Children)
            {
                var scale = transform as ScaleTransform;
                if (scale == null) continue;
                scale.ScaleX += (e.Delta > 0 ? 0.1 : -0.1);
                scale.ScaleY += (e.Delta > 0 ? 0.1 : -0.1);
            }
        }

        private static ControlInfo[] Dump(Window win)
        {
            var sb = new List<ControlInfo>();
            var winc = new ControlInfo
            {
                Text = win.Title,
                Position = new Point(0, 0),
                Size = new Size(win.ActualWidth, win.ActualHeight)
            };
            sb.Add(winc);
            DumpChild(win, 1, winc, sb, win);
            return sb.ToArray();
        }

        private static void DumpChild(DependencyObject target, int indent, ControlInfo parent, List<ControlInfo> result, Window win)
        {
            try
            {
                var children = LogicalTreeHelper.GetChildren(target);
                foreach (var child in children)
                {
                    var frameworkElement = child as Visual;
                    if (frameworkElement == null)
                        continue;
                    DumpControl(frameworkElement, indent, result, win);
                    var cc = result.Last();
                    parent.Children.Add(cc);
                    DumpChild(frameworkElement, indent + 1, cc, result, win);
                }
            }
            catch (Exception)
            {
                //                MessageBox.Show(ex.ToString());
            }
        }

        private static void DumpControl(DependencyObject dep, int indent, List<ControlInfo> result, Window win)
        {
            var fe = dep as FrameworkElement;
            var control = dep as Control;

            var ci = new ControlInfo
            {
                ControlType = dep.GetType().ToString(),
                Depth = indent
            };
            result.Add(ci);
            try
            {
                Point locationFromWindow = fe.TranslatePoint(new Point(0, 0), win);
                ci.Position = locationFromWindow;
            }
            catch (Exception)
            {
            }
            if (fe != null)
            {
                ci.Size = new Size(fe.ActualWidth, fe.ActualHeight);
                ci.TexturePath = RenderControl(fe, ci);
            }

            if (control != null)
            {
                ci.Size = new Size(control.ActualWidth, control.ActualHeight);
                ci.Name = control.Name;
            }
        }

        private static int index = 1;

        private static string RenderControl(FrameworkElement control, ControlInfo ci)
        {
            var width = (int)control.ActualWidth;
            var height = (int)control.ActualHeight;
            if (width == 0 || height == 0) return null;

            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(control);

            // Encoding the RenderBitmapTarget as a PNG file.
            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtb));
            var path = @"c:\temp22\" + ci.ControlType + index++ + ".png";
            using (Stream stm = File.Create(path))
            {
                png.Save(stm);
            }
            return path;
        }

        private Dictionary<ControlInfo, WirePolyline> Map;

        private void OnSelectApp(object sender, RoutedEventArgs e)
        {
            var dlg = new SelectAppWindow();
            var ret = dlg.ShowDialog() ?? false;
            if (ret == true)
            {
                var pid = dlg.SelectedProcess.Process.Id;
                var process = Process.GetProcessById(pid);
                var app = new WindowsAppFriend(process);

                dynamic win = app.Type<Application>().Current.MainWindow;
                WindowsAppExpander.LoadAssemblyFromFile(app, GetType().Assembly.Location);
                var sb = (ControlInfo[])GetInvoker(app).Dump(win);
                Map = new Dictionary<ControlInfo, WirePolyline>();
                ViewPort.Children.Clear();
                var root = sb.First();
                foreach (var c in sb)
                {
                    var frame = CreateControlFrameView(c, root);
                    Map.Add(c, frame);
                    if (c.TexturePath != null)
                    {
                        if (c.Children.Count == 0)
                        {
                            var body = CreateControlBodyView(c, root);
                            var body2 = CreateControlBodyView2(c, root);
                            ViewPort.Children.Add(body);
                            ViewPort.Children.Add(body2);
                        }
                    }
                    ViewPort.Children.Add(frame);
                }
                Tree.ItemsSource = root.Children;
            }
        }

        private Billboard CreateControlBodyView(ControlInfo c, ControlInfo root)
        {
            var left = c.Position.X / root.Size.Width;
            var top = c.Position.Y / root.Size.Height;
            var right = left + c.Size.Width / root.Size.Width;
            var bottom = top + c.Size.Height / root.Size.Height;

            left *= 5;
            top *= 5;
            right *= 5;
            bottom *= 5;

            var hw = 2.5;
            var hh = 2.5;
            left -= hw;
            right -= hw;
            top -= hh;
            bottom -= hh;

            var frame = new Billboard
            {
                LowerLeft = new Point3D(left, -bottom, c.Depth * 0.2),
                LowerRight = new Point3D(right, -bottom, c.Depth * 0.2),
                UpperLeft = new Point3D(left, -top, c.Depth * 0.2),
                UpperRight = new Point3D(right, -top, c.Depth * 0.2),
                Content = new AmbientLight(Colors.White)
            };
            return frame;
        }

        private Billboard CreateControlBodyView2(ControlInfo c, ControlInfo root)
        {
            var left = c.Position.X / root.Size.Width;
            var top = c.Position.Y / root.Size.Height;
            var right = left + c.Size.Width / root.Size.Width;
            var bottom = top + c.Size.Height / root.Size.Height;

            left *= 5;
            top *= 5;
            right *= 5;
            bottom *= 5;

            var hw = 2.5;
            var hh = 2.5;
            left -= hw;
            right -= hw;
            top -= hh;
            bottom -= hh;

            var frame = new Billboard
            {
                LowerLeft = new Point3D(left, -bottom, c.Depth * 0.2),
                LowerRight = new Point3D(right, -bottom, c.Depth * 0.2),
                UpperLeft = new Point3D(left, -top, c.Depth * 0.2),
                UpperRight = new Point3D(right, -top, c.Depth * 0.2),
                Material = new DiffuseMaterial(new ImageBrush
                {
                    ImageSource = LoadImage(c.TexturePath)
                }),
            };
            return frame;
        }


        private BitmapImage LoadImage(string p)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(p, UriKind.Absolute);
            bi.EndInit();
            bi.Freeze();
            return bi;
        }

        private WirePolyline CreateControlFrameView(ControlInfo c, ControlInfo root)
        {
            var frame = new WirePolyline
            {
                Thickness = 2,
                Rounding = 4,
                Color = Colors.Gray,
                Points = new Point3DCollection(),
            };
            var left = c.Position.X / root.Size.Width;
            var top = c.Position.Y / root.Size.Height;
            var right = left + c.Size.Width / root.Size.Width;
            var bottom = top + c.Size.Height / root.Size.Height;

            left *= 5;
            top *= 5;
            right *= 5;
            bottom *= 5;

            var hw = 2.5;
            var hh = 2.5;
            left -= hw;
            right -= hw;
            top -= hh;
            bottom -= hh;

            frame.Points.Add(new Point3D(left, -top, c.Depth * 0.2));
            frame.Points.Add(new Point3D(right, -top, c.Depth * 0.2));
            frame.Points.Add(new Point3D(right, -bottom, c.Depth * 0.2));
            frame.Points.Add(new Point3D(left, -bottom, c.Depth * 0.2));
            frame.Points.Add(new Point3D(left, -top, c.Depth * 0.2));

            return frame;
        }

        private dynamic GetInvoker(WindowsAppFriend app)
        {
            return app.Type(GetType());
        }

        private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var cvm = e.NewValue as ControlInfo;
            if (cvm == null) return;
            if (Map == null) return;

            if (!Map.ContainsKey(cvm)) return;

            foreach (var f in Map.Values)
            {
                f.Color = Colors.Gray;
            }
            var frame = Map[cvm];
            frame.Color = Colors.Red;

        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1 && e.IsDown)
            {
                Reset();
            }
        }

        public void Reset()
        {
            ViewPort.Camera.ClearValue(Camera.TransformProperty);
        }

        private void OnShowLicence(object sender, RoutedEventArgs e)
        {
            var dlg = new LicenseWindow();
            dlg.ShowDialog();
        }
    }
}
