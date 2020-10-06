using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MusicImageGetter
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page , INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        BitmapImage Thumbnail = new BitmapImage();
        StorageItemThumbnail BackUpThumbnail;
        StorageFile file;
        MusicProperties musicProperties;
        string albumName = "";

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void OnPropertyChanged([CallerMemberName]string PropertyName = "")
        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(PropertyName));
        }

        public BitmapImage MusicAlbumArt
        {
            get => Thumbnail;
            set
            {
                Thumbnail = value;
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

        private async void GetFile(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".mp3");
            file = await fileOpenPicker.PickSingleFileAsync();
            FileHandle(file);
        }

        private async void FileHandle(StorageFile file)
        {
            if (file != null)
            {
                saveFile.IsEnabled = true;
                BitmapImage image = new BitmapImage();
                musicProperties = await file.Properties.GetMusicPropertiesAsync();
                await image.SetSourceAsync(await file.GetScaledImageAsThumbnailAsync(ThumbnailMode.SingleItem));
                BackUpThumbnail = await file.GetScaledImageAsThumbnailAsync(ThumbnailMode.SingleItem);
                if (string.IsNullOrWhiteSpace(musicProperties.Album) != true)
                {
                    MusicAlbumName = $"专辑: {musicProperties.Album}";
                }
                else
                {
                    MusicAlbumName = "专辑: 未知专辑";
                }
                if (notifyUserTextBlock.Text != "目前暂时不支持多个文件拖拽，将只显示第一个文件。")
                {
                    notifyUserTextBlock.Text = "";
                }
                MusicAlbumArt = image;
                viewImage.Visibility = Visibility.Visible;
            }
            else
            {
                notifyUserTextBlock.Text = "似乎已取消操作。";
            }
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MusicAlbumName) == false)
            {
                SaveFileAsync(file);
            }
        }

        private async void SaveFileAsync(StorageFile file)
        {
            FileSavePicker fileSavePicker = new FileSavePicker();
            string fileName = musicProperties.Album.Replace("?", string.Empty).Replace(":", string.Empty).Replace("\\", string.Empty).Replace("/", string.Empty).Replace("*", string.Empty).Replace("|", string.Empty).Replace("\"", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty);
            fileSavePicker.SuggestedFileName = fileName;
            fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileSavePicker.FileTypeChoices.Add("PNG文件", new List<string>() { ".png"});
            fileSavePicker.FileTypeChoices.Add("JPG文件", new List<string>() { ".jpg"});
            StorageFile storageFile = await fileSavePicker.PickSaveFileAsync();
            if (storageFile != null)
            {
                CachedFileManager.DeferUpdates(storageFile);
                var image = file == null ? BackUpThumbnail : await file.GetScaledImageAsThumbnailAsync(ThumbnailMode.SingleItem);
                var writingStream = await storageFile.OpenStreamForWriteAsync();
                await image.AsStreamForRead().CopyToAsync(writingStream);
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(storageFile);
                if (status == FileUpdateStatus.Complete || status == FileUpdateStatus.CompleteAndRenamed)
                {
                    notifyUserTextBlock.Text = "已成功保存。";
                }
                else
                {
                    notifyUserTextBlock.Text = $"保存失败! 原因:{status}";
                }
                writingStream.Dispose();
            }
            else
            {
                notifyUserTextBlock.Text = "似乎已取消操作。";
            }
        }

        private async void MailTo(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("mailto:stevemc123456@outlook.com"));
        }

        private async void GoToGithub(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/Baka632/Live-Album-Art-Viewer"));
        }

        private async void ViewImage(object sender, RoutedEventArgs e)
        {
            ImageViewContentDialog contentDialog = new ImageViewContentDialog();
            contentDialog.UpdateImageInfo(MusicAlbumArt, MusicAlbumName);
            await contentDialog.ShowAsync();
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            var defer = e.GetDeferral();
            try
            {
                DataPackageView dpv = e.DataView;
                if (dpv.Contains(StandardDataFormats.StorageItems))
                {
                    var files = await dpv.GetStorageItemsAsync();
                    if (files.Count > 1)
                    {
                        FileHandle((StorageFile)files.First());
                        notifyUserTextBlock.Text = "目前暂时不支持多个文件拖拽，将只显示第一个文件。";
                    }
                    else if(files.Count == 1)
                    {
                        FileHandle((StorageFile)files.First());
                    }
                }
            }
            finally
            {
                defer.Complete();
            }
        }

        private async void Grid_DragOver(object sender, DragEventArgs e)
        {
            var deferral = e.GetDeferral();
            DataPackageView dataview = e.DataView;
            if (dataview.Contains(StandardDataFormats.StorageItems))
            {
                var items = await dataview.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    IStorageItem item = items[0];
                    StorageFile storageFile = item as StorageFile;
                    if (item.IsOfType(StorageItemTypes.File) && storageFile.ContentType == "audio/mpeg")
                    {
                        e.AcceptedOperation = DataPackageOperation.Copy;
                    }
                    else
                    {
                        e.AcceptedOperation = DataPackageOperation.None;
                    }
                }
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
            deferral.Complete();
        }
    }
}
