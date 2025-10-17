using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace BeforeAfterImageView;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Viewer.LeftSource = new Bitmap("Assets/image_1.jpg");
        Viewer.RightSource = new Bitmap("Assets/image_2.jpg");
    }
}