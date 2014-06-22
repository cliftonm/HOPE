using System;
using System.Web;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace AlchemyAPI
{
    public enum ImagePostMode
    {
    	NotRaw,
    	Raw
    }

    public static class ImagePostModeHelper
    {
        public static string ToString(ImagePostMode mode)
        {
            switch (mode)
            {
            case ImagePostMode.Raw:
                return "raw";
            case ImagePostMode.NotRaw:
                return "not-raw";
            default:
                throw new ArgumentException("Invalid ImagePostMode");
            }
        }
    }

	public class AlchemyAPI_RankedImageKeywords : AlchemyAPI_BaseParams
	{
		public const long MaxImageSize = 1024 * 1024; // 1 MB size limit

		public string ImageURL { get; set; }
		public PageImageMode? ExtractMode { get; set; }

		public byte[] ImageData { get; set; }
		public ImagePostMode? PostMode { get; set; }

        public bool UsePost { get; private set; }

        /// <summary>
        /// Sets the <see cref="ImageData"/> property using the already in-memory bitmap.
        /// This function will attempt to downsize the image to not exceed
        /// the <see cref="MaxImageSize"/> constant.
        /// </summary>
        /// <param name='bmp'>
        /// The image to be passed
        /// </param>
		public void SetImage(Bitmap bmp)
        {
            if (bmp == null)
            {
                ImageData = null;
                return;
            }

            float scalePct = 1.0f;

            bool success = false;

            while (!success && scalePct >= 0.1f)
            {
                Bitmap working = null;

                try
                {
                    if (scalePct == 1.0f)
                        working = bmp;
                    else
                    {
                        working = new Bitmap((int)Math.Ceiling(bmp.Width * scalePct),
                                             (int)Math.Ceiling(bmp.Height * scalePct));

                        // Create a graphics object and render the source image
                        // into the destination image
                        using (var graphics = Graphics.FromImage(working))
                        {
                            Rectangle destRect = new Rectangle(0, 0, working.Width, working.Height);

                            graphics.DrawImage(bmp, destRect);
                        }
                    }

                    // Create a memory stream to write the bitmap into
                    using (var byteStream = new MemoryStream())
                    {
                        // Save the image in Jpeg format
                        working.Save(byteStream, ImageFormat.Jpeg);

                        byteStream.Position = 0;

                        if (byteStream.Length < MaxImageSize)
                        {
                            ImageData = byteStream.GetBuffer();
                            success = true;
                        }
                        else
                            scalePct -= 0.1f;
                    }


                } 
                finally
                {
                    if (working != bmp && working != null)
                        working.Dispose();
                }
            }

            if (!success)
                throw new ArgumentException("Unable to downsample the image enough to not exceed the size limit");

            PostMode = ImagePostMode.Raw;
		}
        /// <summary>
        /// Sets the <see cref="ImageData"/> property to be the image on disk
        /// at the specified file path. If the image size exceeds the <see cref="MaxImageSize"/>
        /// constant, the image is downsized automatically
        /// </summary>
        /// <param name='imageFile'>
        /// Path to the image file on disk.
        /// </param>
        public void SetImage(string imageFile)
        {
            FileInfo info = new FileInfo (imageFile);

            if (info.Length > MaxImageSize)
            {
                // If the image is too large, then load it into
                // an in-memory bitmap and use the downsampling
                // function to set the bytes
                using (Bitmap bmp = (Bitmap)Image.FromFile(imageFile))
                {
                    SetImage(bmp);
                }
            }
            else
            {
                // The image is small enough to just be read
                // into the ImageData buffer
                ImageData = File.ReadAllBytes(imageFile);
            }

            PostMode = ImagePostMode.Raw;
        }

        public override string getParameterString()
        {
            string retString = base.getParameterString();

            if (ExtractMode != null)
                retString += "&extractMode=" + PageImageModeHelper.ToString(ExtractMode.Value);

            if (ImageURL != null)
                retString += "&url=" + HttpUtility.UrlEncode(ImageURL);

            if (PostMode != null)
            {
                retString += "&imagePostMode=" + ImagePostModeHelper.ToString(PostMode.Value);

                if (PostMode.Value == ImagePostMode.NotRaw)
                {
                    retString += "&image=" + HttpUtility.UrlEncode(ImageData);
                    UsePost = false;
                }
                else
                    UsePost = true;
            }
            else
                UsePost = false;

            return retString;
        }

        public override byte[] GetPostData()
        {
            if (UsePost)
                return ImageData;
            else
                return null;
        }
	}
}

