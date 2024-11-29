using System.Drawing;
using FFMpegCore.Pipes;

namespace FFMpegCore.Extensions.System.Drawing.Common
{
    public static class FFMpegImage
    {
        /// <summary>
        ///     Saves a 'png' thumbnail to an in-memory bitmap
        /// </summary>
        /// <param name="input">Source video file.</param>
        /// <param name="captureTime">Seek position where the thumbnail should be taken.</param>
        /// <param name="size">Thumbnail size. If width or height equal 0, the other will be computed automatically.</param>
        /// <param name="streamIndex">Selected video stream index.</param>
        /// <param name="inputFileIndex">Input file index</param>
        /// <returns>Bitmap with the requested snapshot.</returns>
        //public static Bitmap Snapshot(string input, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null, int inputFileIndex = 0)
        //{
        //    var source = FFProbe.Analyse(input);
        //    var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(input, source, size, captureTime, streamIndex, inputFileIndex);
        //    using var ms = new MemoryStream();

        //    arguments
        //        .OutputToPipe(new StreamPipeSink(ms), options => outputOptions(options
        //            .ForceFormat("rawvideo")))
        //        .ProcessSynchronously();

        //    ms.Position = 0;
        //    using var bitmap = new Bitmap(ms);
        //    return bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
        //}
        public static Bitmap Snapshot(
    string input,
    Size? size = null,
    TimeSpan? captureTime = null,
    int? streamIndex = null,
    int inputFileIndex = 0)
        {
            // Analyze the input file
            var source = FFProbe.Analyse(input);

            // Build arguments and output options
            var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(
                input,
                source,
                size,
                captureTime,
                streamIndex,
                inputFileIndex);

            using var ms = new MemoryStream();

            // Run FFmpeg to capture a single frame
            var result = arguments
                .OutputToPipe(new StreamPipeSink(ms), options => outputOptions(options
                    .WithVideoCodec("mjpeg") // Ensure output is in a compatible format (JPEG)
                    .ForceFormat("image2"))) // Use 'image2' for single image output
                .ProcessSynchronously();

            if (!result)
            {
                throw new InvalidOperationException("FFmpeg failed to generate snapshot.");
            }

            // Reset stream position to the beginning
            ms.Position = 0;

            // Create a Bitmap from the MemoryStream
            try
            {
                using var bitmap = new Bitmap(ms);
                return bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create bitmap from FFmpeg output.", ex);
            }
        }

        /// <summary>
        ///     Saves a 'png' thumbnail to an in-memory bitmap
        /// </summary>
        /// <param name="input">Source video file.</param>
        /// <param name="captureTime">Seek position where the thumbnail should be taken.</param>
        /// <param name="size">Thumbnail size. If width or height equal 0, the other will be computed automatically.</param>
        /// <param name="streamIndex">Selected video stream index.</param>
        /// <param name="inputFileIndex">Input file index</param>
        /// <returns>Bitmap with the requested snapshot.</returns>
        public static async Task<Bitmap> SnapshotAsync(string input, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null, int inputFileIndex = 0)
        {
            var source = await FFProbe.AnalyseAsync(input).ConfigureAwait(false);
            var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(input, source, size, captureTime, streamIndex, inputFileIndex);
            using var ms = new MemoryStream();

            await arguments
                .OutputToPipe(new StreamPipeSink(ms), options => outputOptions(options
                    .ForceFormat("rawvideo")))
                .ProcessAsynchronously();

            ms.Position = 0;
            return new Bitmap(ms);
        }
    }
}
