using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace MusicImageGetter
{
    public sealed partial class ImageViewContentDialog : ContentDialog , INotifyPropertyChanged
    {
        BitmapImage bitmapImage = new BitmapImage();
        string pixelHeightWidth = "";
        string albumName = "";
        public event PropertyChangedEventHandler PropertyChanged;

        public BitmapImage AlbumArt
        {
            get => bitmapImage;
            set
            {
                bitmapImage = value;
                OnPropertyChanged();
            }
        }

        public string MusicAlbumName
        {
            get => albumName;
            set
            {
                albumName = value;
                OnPropertyChanged();
            }
        }

        public ImageViewContentDialog()
        {
            this.InitializeComponent();
        }

        private void OnPropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public void UpdateImageInfo(BitmapImage image, string albumName)
        {
            AlbumArt = image;
            PixelHeightWidth = $"分辨率: {image.PixelHeight} x {image.PixelWidth}";
            MusicAlbumName = albumName;
        }

        public string PixelHeightWidth
        {
            get => pixelHeightWidth;
            set
            {
                pixelHeightWidth = value;
                OnPropertyChanged();
            }
        }
    }
}
