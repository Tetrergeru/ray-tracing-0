using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace RayTracing
{
    class Form : System.Windows.Forms.Form
    {
        private readonly PictureBox _screen;
        
        private World world = new World();

        private const int ScreenWidth = 500;
        private const int ScreenHeight = 500;
        
        public Form()
        {
            Width = ScreenWidth + 70;
            Height = ScreenHeight + 70;
            
            _screen = new PictureBox
            {
                Width = ScreenWidth,
                Height = ScreenHeight,
                Left = 20,
                Top = 20,
                BackgroundImageLayout = ImageLayout.Zoom,
            };
            _screen.BackgroundImage = world.RayTrace(ScreenWidth, ScreenHeight);
            Controls.Add(_screen);
        }
    }
}