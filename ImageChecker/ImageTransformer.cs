using Color = System.Drawing.Color;
using System.Drawing;


namespace ImageChecker.core
{
    public class ImageTransformer
    {
        public Bitmap CurrentImage { get; set; }
        public Bitmap ProcessedImage { get; set; }
        private int _currentRotationAngle = 0;
        private float _zoomFactor = 1f;
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

        public void Zoom(float zoomFactor)
        {
            _zoomFactor = zoomFactor;

            int newWidth = (int)(CurrentImage.Width * zoomFactor);
            int newHeight = (int)(CurrentImage.Height * zoomFactor);

            Bitmap zoomedImage = new Bitmap(newWidth, newHeight);

            zoomedImage.SetResolution(CurrentImage.HorizontalResolution, CurrentImage.VerticalResolution);

            using (Graphics g = Graphics.FromImage(zoomedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                g.DrawImage(CurrentImage, 0, 0, newWidth, newHeight);
            }
            ProcessedImage = zoomedImage;
        }
    }
}
