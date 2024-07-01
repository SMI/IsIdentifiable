using FellowOakDicom.Imaging.Render;
using FellowOakDicom.Imaging;
using FellowOakDicom.IO;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;

namespace IsIdentifiable;

/// <inheritdoc cref="IImageManager" />
public sealed class IsIdentifiableImageManager : ImageBase<Image<Bgra32>>, IImageManager
{
    /// <inheritdoc />
    public IImage CreateImage(int width, int height) => new IsIdentifiableImageManager(width, height);

    /// <inheritdoc />
    public IsIdentifiableImageManager() : this(0, 0)
    {
    }

    /// <inheritdoc />
    public IsIdentifiableImageManager(int width, int height) : this(width, height,
        new PinnedIntArray(width * height), null)
    {
    }

    private IsIdentifiableImageManager(int width, int height, PinnedIntArray pixels, Image<Bgra32>? image) : base(
        width, height, new PinnedIntArray(pixels.Data), image?.Clone())
    {
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _image?.Dispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc />
    public override unsafe void Render(int components, bool flipX, bool flipY, int rotation)
    {
        Span<byte> data = new(_pixels.Pointer.ToPointer(), _pixels.ByteSize);
        _image = Image.LoadPixelData<Bgra32>(data, _width, _height);

        if (flipX && flipY)
            // flipping both horizontally and vertically is equal to rotating 180 degrees
            rotation += 180;

        var flipMode = flipX switch
        {
            true when flipY => FlipMode.None,
            true => FlipMode.Horizontal,
            _ => flipY ? FlipMode.Vertical : FlipMode.None
        };

        var rotationMode = (rotation % 360) switch
        {
            90 => RotateMode.Rotate90,
            180 => RotateMode.Rotate180,
            270 => RotateMode.Rotate270,
            _ => RotateMode.None
        };

        if (flipMode != FlipMode.None || rotationMode != RotateMode.None)
            _image.Mutate(x => x.RotateFlip(rotationMode, flipMode));
    }

    /// <inheritdoc />
    public override void DrawGraphics(IEnumerable<IGraphic> graphics)
    {
        foreach (var graphic in graphics)
        {
            var layer = (graphic.RenderImage(null) as IsIdentifiableImageManager)?._image;
            _image.Mutate(ctx => ctx
                .DrawImage(layer ?? throw new InvalidOperationException("Mixed image types in fo-dicom Image?!"),
                    new Point(graphic.ScaledOffsetX, graphic.ScaledOffsetY), 1));
        }
    }

    /// <inheritdoc />
    public override IImage Clone() =>
        new IsIdentifiableImageManager(_width, _height, new PinnedIntArray(_pixels.Data), _image?.Clone());

    /// <summary>
    /// Expose the internal raw SharpImage object for direct manipulation.
    /// Do NOT dispose of it directly, as it will be disposed of by the ImageManager.
    /// </summary>
    /// <returns></returns>
    public Image<Bgra32> GetSharpImage() => _image;
}
