using System.Diagnostics.Metrics;
using System.Drawing;
using System.Globalization;
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
using ImageChecker;
using ImageChecker.core;
using Microsoft.Win32;



namespace ImageComparison
{
    public partial class MainWindow : Window
    {

        private readonly System.Windows.Controls.Image[] _imageSlots;
        private readonly TextBlock[] _fileInfoSlots;
        private readonly Button[] _deleteButtons;
        private ImageTransformer[] _imageTransformers;

        private DateTime _lastDrawTime = DateTime.MinValue;

        int count = 1;
        public MainWindow()
        {
            InitializeComponent();

            _imageSlots = new[] { Image1, Image2, Image3, Image4 };
            _fileInfoSlots = new[] { FileInfo1, FileInfo2, FileInfo3, FileInfo4 };
            _deleteButtons = new[] { DeleteFirstImageBtn, DeleteSecondImageBtn, DeleteThirdImageBtn, DeleteFourthImageBtn };
            _imageTransformers = new ImageTransformer[_imageSlots.Length];


        }

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                DisplayImage(filePath);
            }
        }

        private void DisplayImage(string filePath)
        {
            for (int i = 0; i < _imageSlots.Length; i++)
            {
                if (_imageSlots[i].Source == null)
                {
                    _imageSlots[i].Source = new BitmapImage(new Uri(filePath));
                    _fileInfoSlots[i].Text = $"File: {System.IO.Path.GetFileName(filePath)}: {new System.IO.FileInfo(filePath).Length / 1024} KB";
                    _deleteButtons[i].Visibility = Visibility.Visible;

                    // Initialize ImageTransformer for this image
                    _imageTransformers[i] = new ImageTransformer(filePath);
                    return;
                }
            }

            MessageBox.Show("All image blocks are filled.");
        }

        private void DeleteImage_Click(object sender, RoutedEventArgs e)
        {
            // Determine which delete button was clicked
            for (int i = 0; i < _deleteButtons.Length; i++)
            {
                if (sender == _deleteButtons[i])
                {
                    _imageSlots[i].Source = null;
                    _fileInfoSlots[i].Text = string.Empty;
                    _deleteButtons[i].Visibility = Visibility.Hidden;
                    _imageTransformers[i] = null;
                    return;
                }
            }
        }
        private void ProcessAllImages_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < _imageTransformers.Length; i++)
            {
                if (_imageTransformers[i] != null)
                {
                    _imageTransformers[i].DrawStaticGuidingLines();

                    _imageSlots[i].Source = BitmapToImageSource(_imageTransformers[i].ProcessedImage);
                }
            }

            // MessageBox.Show("Processed all loaded images!");
        }

        private void ProcessAllImages_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < _imageTransformers.Length; i++)
            {
                if (_imageTransformers[i] != null)
                {
                    _imageTransformers[i].restoreProcessedImage();
                    _imageSlots[i].Source = BitmapToImageSource(_imageTransformers[i].ProcessedImage);
                }
            }

            // MessageBox.Show("Reset all images to their original state.");
        }
        private BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new System.IO.MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string angleString && int.TryParse(angleString, out int angle))
            {
                RotateImage(angle);
            }
        }

        private void RotateButton_Click2(object sender, RoutedEventArgs e)
        {
            string angleText = AngleTextBox.Text;
            if (sender is Button button && int.TryParse(angleText, out int angle))
            {
                RotateImage2(angle);
            }
        }

        private void AngleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Check if the angle value has changed (if it's not the first time)
            if (e.OldValue != e.NewValue)
            {
                // Update rotation based on slider value
                int angle = (int)e.NewValue;
                RotateImage2(angle);  // Call rotation method with the angle
            }
        }

        private void RotateImage2(int angle)
        {
            for (int i = 0; i < _imageTransformers.Length; i++)
            {
                if (_imageTransformers[i] != null)
                {
                    _imageTransformers[i].Rotate2(angle);
                    _imageSlots[i].Source = BitmapToImageSource(_imageTransformers[i].ProcessedImage);
                }
            }

            // MessageBox.Show($"Rotated all images by {angle}°.");

        }
        private void RotateImage(int angle)
        {
            for (int i = 0; i < _imageTransformers.Length; i++)
            {
                if (_imageTransformers[i] != null)
                {
                    _imageTransformers[i].Rotate(angle);
                    _imageSlots[i].Source = BitmapToImageSource(_imageTransformers[i].ProcessedImage);
                }
            }

            // MessageBox.Show($"Rotated all images by {angle}°.");
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (WiteLinesCheckBox.IsChecked != true && DiagonalLinesCheckBox.IsChecked != true && CircleLinesCheckBox.IsChecked != true)
            {
                return;
            }
            //var currentTime = DateTime.Now;
            //if ((currentTime - _lastDrawTime).TotalMilliseconds < 100)
            //   return;
            //_lastDrawTime = currentTime;

            var image = sender as System.Windows.Controls.Image;
            if (image == null) return;
            var imagePosition = e.GetPosition(image);

            var source = image.Source as System.Windows.Media.Imaging.BitmapSource;

            if (source == null) return;

            // Calculate the scaling factors

            double scaleX = source.PixelWidth / image.ActualWidth;

            double scaleY = source.PixelHeight / image.ActualHeight;


            var pixelX = imagePosition.X * scaleX;

            var pixelY = imagePosition.Y * scaleY;
            for (int i = 0; i < _imageSlots.Length; i++)
            {
                if (WiteLinesCheckBox.IsChecked == true)
                {
                    DrawGuidingLines(new System.Windows.Point(pixelX, pixelY), i);
                }
                else if (DiagonalLinesCheckBox.IsChecked == true)
                {
                    DrawDiagonalLines(new System.Windows.Point(pixelX, pixelY), i);
                }
                else if (CircleLinesCheckBox.IsChecked == true)  // Check for circle checkbox
                {
                    DrawCircle(new System.Windows.Point(pixelX, pixelY), i);
                }
            }
        }

        private void DrawGuidingLines(System.Windows.Point cursorPosition, int imageIndex)
        {
            var transformer = _imageTransformers[imageIndex];
            if (transformer == null) return;
            transformer.DrawCursorGuidingLines(cursorPosition);
            _imageSlots[imageIndex].Source = BitmapToImageSource(transformer.ProcessedImage);
        }

        private void DrawDiagonalLines(System.Windows.Point cursorPosition, int imageIndex)
        {
            var transformer = _imageTransformers[imageIndex];
            if (transformer == null) return;
            transformer.DrawDiagonalLines(cursorPosition);
            _imageSlots[imageIndex].Source = BitmapToImageSource(transformer.ProcessedImage);
        }

        private void DrawCircle(System.Windows.Point cursorPosition, int imageIndex)
        {
            var transformer = _imageTransformers[imageIndex];
            if (transformer == null) return;

            // Draw the circle (radius is fixed or you can adjust it as needed)
            transformer.DrawCursorCircle(cursorPosition, 50);  // 50 is the radius of the circle
            _imageSlots[imageIndex].Source = BitmapToImageSource(transformer.ProcessedImage);
        }

        private void ZoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string zoomFactorString && float.TryParse(zoomFactorString, out float zoomFactor))
            {
                ZoomImage(zoomFactor);
            }
        }

        private void ZoomImage(float zoomFactor)
        {
            
            for (int i = 0; i < _imageTransformers.Length; i++)
            {
                if (_imageTransformers[i] != null)
                {
                    CursorPositionText.Text = zoomFactor.ToString();
                    _imageTransformers[i].Zoom(zoomFactor);
                    _imageSlots[i].Source = BitmapToImageSource(_imageTransformers[i].ProcessedImage);
                }
            }

            
        }

    }
}
