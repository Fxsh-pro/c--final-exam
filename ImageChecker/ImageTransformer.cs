using Color = System.Drawing.Color;
using System.Drawing;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;


namespace ImageChecker.core
{
    public class ImageTransformer
    {
        public Bitmap CurrentImage { get; set; }
        public Bitmap ProcessedImage { get; set; }
        private int _currentRotationAngle = 0;
        private double _zoomFactor = 1f;
        private const double MinZoom = 0.1;  // Minimum zoom
        private const double MaxZoom = 3.0;  // Maximum zoom
        public ImageTransformer(string filePath)
        {
            CurrentImage = new Bitmap(filePath);
            ProcessedImage = (Bitmap)CurrentImage.Clone();
        }

        public void restoreProcessedImage()
        {
            ProcessedImage = (System.Drawing.Bitmap)CurrentImage.Clone();
            _currentRotationAngle = 0;
        }

        public void DrawStaticGuidingLines()
        {
            using (Graphics g = Graphics.FromImage(ProcessedImage))
            {
                System.Drawing.Pen guidePen = new System.Drawing.Pen(Color.Red, 5);

                // Draw lines from corners
                g.DrawLine(guidePen, 0, 0, ProcessedImage.Width, ProcessedImage.Height);
                g.DrawLine(guidePen, ProcessedImage.Width, 0, 0, ProcessedImage.Height);
            }
        }

        public void Rotate(int angle)
        {

            angle = angle % 360;
            _currentRotationAngle = (_currentRotationAngle + angle) % 360;

            ProcessedImage.RotateFlip(GetRotateFlipType(angle));
        }
        public void Rotate2(float angle)
        {
            // Ensure the angle is within 0 to 360
            angle = angle % 360;

            // Create a new bitmap to store the rotated image
            Bitmap rotatedImage = new Bitmap(CurrentImage.Width, CurrentImage.Height);
            rotatedImage.SetResolution(CurrentImage.HorizontalResolution, CurrentImage.VerticalResolution);

            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                // Set the rotation point to the center of the image
                g.TranslateTransform(CurrentImage.Width / 2, CurrentImage.Height / 2);

                // Rotate the image
                g.RotateTransform(angle);

                // Restore the image position after rotation
                g.TranslateTransform(-CurrentImage.Width / 2, -CurrentImage.Height / 2);

                // Draw the original image onto the rotated image
                g.DrawImage(CurrentImage, 0, 0);
            }

            // Update the ProcessedImage to the rotated image
            ProcessedImage = rotatedImage;
            _currentRotationAngle = (int)angle;
        }

        private RotateFlipType GetRotateFlipType(int angle)
        {
            return angle switch
            {
                90 => RotateFlipType.Rotate90FlipNone,
                180 => RotateFlipType.Rotate180FlipNone,
                270 => RotateFlipType.Rotate270FlipNone,
                _ => RotateFlipType.RotateNoneFlipNone,
            };
        }

        public void DrawCursorGuidingLines(System.Windows.Point cursorPosition)
        {
            restoreProcessedImage();
            using (Graphics g = Graphics.FromImage(ProcessedImage))
            {
                var guidePen = new System.Drawing.Pen(System.Drawing.Color.Red, 5);

                g.DrawLine(guidePen, (float)cursorPosition.X, 0, (float)cursorPosition.X, ProcessedImage.Height);
                g.DrawLine(guidePen, 0, (float)cursorPosition.Y, ProcessedImage.Width, (float)cursorPosition.Y);
            }
        }

        public void DrawDiagonalLines(System.Windows.Point cursorPosition)
        {
            restoreProcessedImage();
            using (Graphics g = Graphics.FromImage(ProcessedImage))
            {
                // Create a blue pen for the diagonal lines
                var guidePen = new System.Drawing.Pen(System.Drawing.Color.Blue, 5);

                g.DrawLine(guidePen, (float)cursorPosition.X, 0, (float)cursorPosition.X, (float)ProcessedImage.Height); // Vertical line
                g.DrawLine(guidePen, 0, (float)cursorPosition.Y, (float)ProcessedImage.Width, (float)cursorPosition.Y); // Horizontal line
            }
        }

        public void DrawCursorCircle(System.Windows.Point cursorPosition, int radius)
        {
            restoreProcessedImage();
            using (Graphics g = Graphics.FromImage(ProcessedImage))
            {
                var guidePen = new System.Drawing.Pen(System.Drawing.Color.Blue, 3);  // Adjusted the pen width to 3 for a thinner circle

                int x = (int)(cursorPosition.X - radius / 2);  // Half the radius to center the circle
                int y = (int)(cursorPosition.Y - radius / 2);  // Half the radius to center the circle
                int diameter = radius;  // Reduced the size

                g.DrawEllipse(guidePen, x, y, diameter, diameter);
            }
        }

        public void DrawCursorRectangle(System.Windows.Point cursorPosition, int size)
        {
            restoreProcessedImage();
            using (Graphics g = Graphics.FromImage(ProcessedImage))
            {
                var guidePen = new System.Drawing.Pen(System.Drawing.Color.Red, 3); // Adjust width as needed

                int x = (int)(cursorPosition.X - size / 2);
                int y = (int)(cursorPosition.Y - size / 2);
                int width = size;
                int height = size;

                g.DrawRectangle(guidePen, x, y, width, height);
            }
        }

        public void Zoom(System.Windows.Controls.Image image, double zoomFactor, int delta)
        {
            var transform = image.RenderTransform as ScaleTransform;
            if (transform == null)
            {
                transform = new ScaleTransform(1.0, 1.0);
                image.RenderTransform = transform;
                image.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5); // Центрируем трансформацию
            }

            // Apply zoom based on scroll direction (without needing Ctrl)
            zoomFactor = delta > 0 ? 1.1 : 0.9; // Увеличение/уменьшение на 10%
            transform.ScaleX *= zoomFactor;
            transform.ScaleY *= zoomFactor;

            // Ограничиваем минимальный и максимальный зум
            transform.ScaleX = Math.Max(0.1, Math.Min(1.0, transform.ScaleX));
            transform.ScaleY = Math.Max(0.1, Math.Min(1.0, transform.ScaleY));

            ProcessedImage = ConvertImageToBitmap(image);
        }

        public static Bitmap ConvertImageToBitmap(System.Windows.Controls.Image wpfImage)
        {
            // Create a RenderTargetBitmap to render the image
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                (int)wpfImage.Width,
                (int)wpfImage.Height,
                96, 96,
                PixelFormats.Pbgra32);

            // Create a DrawingVisual to render the image
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                // Draw the Image onto the DrawingContext
                drawingContext.DrawImage(wpfImage.Source, new System.Windows.Rect(0, 0, wpfImage.Width, wpfImage.Height));
            }

            // Render the DrawingVisual into the RenderTargetBitmap
            renderTargetBitmap.Render(drawingVisual);

            // Create a MemoryStream to store the image data
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Encode the RenderTargetBitmap into PNG (you can use other formats as well)
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                encoder.Save(memoryStream);

                // Convert MemoryStream to Bitmap (System.Drawing.Bitmap)
                memoryStream.Seek(0, SeekOrigin.Begin);
                return new Bitmap(memoryStream);
            }
        }
    }
}
