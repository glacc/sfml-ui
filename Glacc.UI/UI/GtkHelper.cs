using Cairo;
using SFML.Graphics;

namespace Glacc.UI
{
    public static class GtkHelper
    {
        /// <summary>
        ///     Copies SFML texture into GTK image surface.
        /// </summary>
        public static void CopyTextureToImageSurface(ref Texture texture, ref ImageSurface surface)
        {
            switch (surface.Format)
            {
                case Format.ARGB32:
                    CopyTextureToImageSurfaceBGRA(ref texture, ref surface);
                    break;
                case Format.RGB24:
                    CopyTextureToImageSurfaceRGB(ref texture, ref surface);
                    break;
            }
        }

        public unsafe static void CopyTextureToImageSurfaceWithoutConversion(ref Texture texture, ref ImageSurface surface)
        {

            Image textureImg = texture.CopyToImage();

            int textureWidth = (int)textureImg.Size.X;
            int textureHeight = (int)textureImg.Size.Y;
            int textureStride = textureWidth * 4;

            int surfaceWidth = surface.Width;
            int surfaceHeight = surface.Height;
            int surfaceStride = surface.Stride;

            bool strideMatches = (textureStride == surfaceStride);

            surface.Flush();

            int width, height, stride;
            byte[] texturePixels = textureImg.Pixels;
            byte* ptrSurfaceData = (byte*)surface.DataPtr;
            if (strideMatches)
            {
                width = surfaceWidth;
                height = Math.Min(textureHeight, surfaceHeight);
                stride = Math.Min(textureStride, surfaceStride);

                int maxOffset = stride * height;
                byte* ptrDst = ptrSurfaceData;

                fixed (byte* ptrSrc = texturePixels)
                {
                    Buffer.MemoryCopy(ptrSrc, ptrDst, maxOffset, maxOffset);
                }
            }
            else
            {
                width = Math.Min(textureWidth, surfaceWidth);
                height = Math.Min(textureHeight, surfaceHeight);
                stride = Math.Min(textureStride, surfaceStride);

                for (int y = 0; y < height; y++)
                {
                    int offsetSrc = textureStride * y;
                    int offsetDst = surfaceStride * y;

                    fixed (byte* ptrSrcFixed = texturePixels)
                    {
                        byte* ptrSrc = ptrSrcFixed + offsetSrc;
                        byte* ptrDst = ptrSurfaceData + offsetDst;

                        Buffer.MemoryCopy(ptrSrc, ptrDst, surfaceStride, textureStride);
                    }
                }
            }

            surface.MarkDirty();
        }

        public unsafe static void CopyTextureToImageSurfaceRGB(ref Texture texture, ref ImageSurface surface)
        {
            Image textureImg = texture.CopyToImage();

            int textureWidth = (int)textureImg.Size.X;
            int textureHeight = (int)textureImg.Size.Y;
            int textureStride = textureWidth * 4;

            int surfaceWidth = surface.Width;
            int surfaceHeight = surface.Height;
            int surfaceStride = surface.Stride;

            bool strideMatches = (textureStride == surfaceStride);

            surface.Flush();

            int width, height;
            byte[] texturePixels = textureImg.Pixels;
            byte* ptrSurfaceData = (byte*)surface.DataPtr;
            if (strideMatches)
            {
                width = surfaceWidth;
                height = Math.Min(textureHeight, surfaceHeight);
                int stride = surfaceStride;

                int offsetSrc = 0;
                int maxOffset = stride * height;
                byte* ptrDst = ptrSurfaceData;
                while (offsetSrc < maxOffset)
                {
                    byte r = texturePixels[offsetSrc++];
                    byte g = texturePixels[offsetSrc++];
                    byte b = texturePixels[offsetSrc++];
                    offsetSrc++;
                    *ptrDst++ = b;
                    *ptrDst++ = g;
                    *ptrDst++ = r;
                    ptrDst++;
                }
            }
            else
            {
                width = Math.Min(textureWidth, surfaceWidth);
                height = Math.Min(textureHeight, surfaceHeight);

                for (int y = 0; y < height; y++)
                {
                    int offsetSrc = textureStride * y;
                    byte* ptrDst = ptrSurfaceData + (surfaceStride * y);
                    for (int x = 0; x < width; x++)
                    {
                        byte r = texturePixels[offsetSrc++];
                        byte g = texturePixels[offsetSrc++];
                        byte b = texturePixels[offsetSrc++];
                        offsetSrc++;
                        *ptrDst++ = b;
                        *ptrDst++ = g;
                        *ptrDst++ = r;
                        ptrDst++;
                    }
                }
            }

            surface.MarkDirty();
        }


        public unsafe static void CopyTextureToImageSurfaceBGRA(ref Texture texture, ref ImageSurface surface)
        {
            Image textureImg = texture.CopyToImage();

            int textureWidth = (int)textureImg.Size.X;
            int textureHeight = (int)textureImg.Size.Y;
            int textureStride = textureWidth * 4;

            int surfaceWidth = surface.Width;
            int surfaceHeight = surface.Height;
            int surfaceStride = surface.Stride;

            bool strideMatches = (textureStride == surfaceStride);

            surface.Flush();

            int width, height;
            byte[] texturePixels = textureImg.Pixels;
            byte* ptrSurfaceData = (byte*)surface.DataPtr;
            if (strideMatches)
            {
                width = surfaceWidth;
                height = Math.Min(textureHeight, surfaceHeight);
                int stride = surfaceStride;

                int offsetSrc = 0;
                int maxOffset = stride * height;
                byte* ptrDst = ptrSurfaceData;
                while (offsetSrc < maxOffset)
                {
                    byte r = texturePixels[offsetSrc++];
                    byte g = texturePixels[offsetSrc++];
                    byte b = texturePixels[offsetSrc++];
                    byte a = texturePixels[offsetSrc++];
                    *ptrDst++ = b;
                    *ptrDst++ = g;
                    *ptrDst++ = r;
                    *ptrDst++ = a;
                }
            }
            else
            {
                width = Math.Min(textureWidth, surfaceWidth);
                height = Math.Min(textureHeight, surfaceHeight);

                for (int y = 0; y < height; y++)
                {
                    int offsetSrc = textureStride * y;
                    byte* ptrDst = ptrSurfaceData + (surfaceStride * y);
                    for (int x = 0; x < width; x++)
                    {
                        byte r = texturePixels[offsetSrc++];
                        byte g = texturePixels[offsetSrc++];
                        byte b = texturePixels[offsetSrc++];
                        byte a = texturePixels[offsetSrc++];
                        *ptrDst++ = b;
                        *ptrDst++ = g;
                        *ptrDst++ = r;
                        *ptrDst++ = a;
                    }
                }
            }

            surface.MarkDirty();
        }
    }
}
