using System.Drawing;
using System.Windows.Forms;

namespace GlslDebug
{
    public partial class ShowImage : Form
    {
        private delegate void VoidBitmapDelegate(Bitmap image);
        public Bitmap currentImage;

        public ShowImage()
        {
            InitializeComponent();
        }

        private void Image_Paint(object sender, PaintEventArgs e)
        {
            if(currentImage != null)
            {
                Graphics g = e.Graphics;
                g.DrawImage(currentImage, new RectangleF(0, 0, pictureBox1.Width, pictureBox1.Height));
            }
        }

        public void UpdateImage(Bitmap image)
        {
            if (pictureBox1.InvokeRequired)
            {
                VoidBitmapDelegate d = new VoidBitmapDelegate(UpdateImage);
                Invoke(d, new object[] { image });
            }
            else
            {
                currentImage = image;
                Refresh();
            }
        }
    }
}
