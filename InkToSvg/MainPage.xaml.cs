using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Svg;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace InkToSvg
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            InkControl.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse |
                                                       CoreInputDeviceTypes.Pen |
                                                       CoreInputDeviceTypes.Touch;

            InkControl.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            InkControl.InkPresenter.StrokesErased += InkPresenter_StrokesErased;

        }

        private async void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            await RenderSvg();
        }

        private async void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            await RenderSvg();
        }

        private async Task RenderSvg()
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                await RenderSvg(stream);
                var image = new SvgImageSource();
                await image.SetSourceAsync(stream);
                ResultSvgImage.Source = image;
            }
        }

        private async void OnSave(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = "New Image",
                FileTypeChoices = { {"Scalable Vector Graphics", new[] { ".svg" } } }
            };

            var file = await savePicker.PickSaveFileAsync();
            if (file == null) return;

            using (var writeStream = (await file.OpenStreamForWriteAsync()).AsRandomAccessStream())
            {
                await RenderSvg(writeStream);

                await writeStream.FlushAsync();
            }
        }

        private async Task RenderSvg(IRandomAccessStream writeStream)
        {
            var sharedDevice = CanvasDevice.GetSharedDevice();

            using (var offscreen = new CanvasRenderTarget(sharedDevice, (float) InkControl.RenderSize.Width, (float) InkControl.RenderSize.Height, 96))
            {
                using (var session = offscreen.CreateDrawingSession())
                {
                    var svgDocument = new CanvasSvgDocument(sharedDevice);

                    svgDocument.Root.SetStringAttribute("viewBox", $"0 0 {InkControl.RenderSize.Width} {InkControl.RenderSize.Height}");

                    foreach (var stroke in InkControl.InkPresenter.StrokeContainer.GetStrokes())
                    {
                        var canvasGeometry = CanvasGeometry.CreateInk(session, new[] {stroke}).Outline(); //.Simplify(CanvasGeometrySimplification.Lines);

                        var pathReceiver = new CanvasGeometryToSvgPathReader();
                        canvasGeometry.SendPathTo(pathReceiver);

                        var element = svgDocument.Root.CreateAndAppendNamedChildElement("path");

                        element.SetStringAttribute("d", pathReceiver.Path);

                        var color = stroke.DrawingAttributes.Color;

                        element.SetColorAttribute("fill", color);
                        //if (stroke.DrawingAttributes.DrawAsHighlighter)
                        //    element.SetFloatAttribute("fill-opacity", .9f);
                    }

                    await svgDocument.SaveAsync(writeStream);
                }
            }
        }
    }
}