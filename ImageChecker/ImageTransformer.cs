using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Color = System.Drawing.Color;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
                g.DrawLine(guidePen, 0, 0, ProcessedImage.Width, ProcessedImage.Height); // Top-left to bottom-right
                g.DrawLine(guidePen, ProcessedImage.Width, 0, 0, ProcessedImage.Height); // Top-right to bottom-left
            }
        }

        public void Rotate(int angle)
        {
            /* Bitmap rotatedImg = (Bitmap)ProcessedImage.Clone(); 
            angle = angle % 360;  
            _currentRotationAngle = (_currentRotationAngle + angle) % 360;
            rotatedImg.RotateFlip(GetRotateFlipType(angle));
            return rotatedImg; */ // Возвращаем новый объект с повернутым изображением
            angle = angle % 360;
            _currentRotationAngle = (_currentRotationAngle + angle) % 360; // Update rotation state

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
                // Create a red pen for the guiding lines
                var guidePen = new System.Drawing.Pen(System.Drawing.Color.Red, 5);

                // Draw the vertical line at the cursor X position
                g.DrawLine(guidePen, (float)cursorPosition.X, 0, (float)cursorPosition.X, ProcessedImage.Height);

                // Draw the horizontal line at the cursor Y position
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

                // Draw the diagonal lines
                g.DrawLine(guidePen, (float)cursorPosition.X, 0, (float)cursorPosition.X, (float)ProcessedImage.Height); // Vertical line
                g.DrawLine(guidePen, 0, (float)cursorPosition.Y, (float)ProcessedImage.Width, (float)cursorPosition.Y); // Horizontal line
            }
        }

        public void DrawCursorCircle(System.Windows.Point cursorPosition, int radius)
        {
            restoreProcessedImage();
            using (Graphics g = Graphics.FromImage(ProcessedImage))
            {
                // Create a blue pen for the circle
                var guidePen = new System.Drawing.Pen(System.Drawing.Color.Blue, 3);  // Adjusted the pen width to 3 for a thinner circle

                // Calculate the rectangle for the circle, centered at cursor position
                int x = (int)(cursorPosition.X - radius / 2);  // Half the radius to center the circle
                int y = (int)(cursorPosition.Y - radius / 2);  // Half the radius to center the circle
                int diameter = radius;  // Reduced the size

                // Draw the circle
                g.DrawEllipse(guidePen, x, y, diameter, diameter);
            }
        }

        public void Zoom(float zoomFactor)
        {
            // Add the input zoom factor to the current zoom factor
            _zoomFactor += zoomFactor;

            // Ensure the zoom factor is within reasonable bounds (e.g., 0.1 to 10)
            _zoomFactor = Math.Max(0.1f, Math.Min(_zoomFactor, 10f));

            // Calculate new width and height based on the updated zoom factor
            int newWidth = (int)(CurrentImage.Width * _zoomFactor);
            int newHeight = (int)(CurrentImage.Height * _zoomFactor);

            // Create a new bitmap with the new dimensions
            Bitmap zoomedImage = new Bitmap(newWidth, newHeight);
            zoomedImage.SetResolution(CurrentImage.HorizontalResolution, CurrentImage.VerticalResolution);

            // Use Graphics to draw the image with scaling applied
            using (Graphics g = Graphics.FromImage(zoomedImage))
            {
                g.Clear(Color.Transparent); // Clear background to prevent any artifacts
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(CurrentImage, 0, 0, newWidth, newHeight);
            }

            // Update ProcessedImage with the zoomed version
            ProcessedImage = zoomedImage;
        }
    }
}
