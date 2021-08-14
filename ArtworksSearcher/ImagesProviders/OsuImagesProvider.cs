﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using ArtworksSearcher.Ex;
using Root.Abstractions;

namespace ArtworksSearcher.ImagesProviders
{
    public class OsuImagesProvider : IImagesProvider
    {
        private readonly string[] _imgExtensions = { ".jpg", ".png", ".jpeg" };
        private readonly DirectoryInfo _songsDir;
        public long MinSize = 0;

        public OsuImagesProvider(string songsDirPath)
        {
            _songsDir = new DirectoryInfo(songsDirPath);
        }

        public OsuImagesProvider(DirectoryInfo songsDir)
        {
            _songsDir = songsDir;
        }

        public IEnumerable<BitmapImage> GetImagesIter(string query)
        {
            foreach (var data in GetBinaryIter(query))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = new MemoryStream(data);
                image.EndInit();
                yield return image;
            }
        }

        public IEnumerable<byte[]> GetBinaryIter(string query)
        {
            foreach (var file in GetFilesIter(query))
                yield return File.ReadAllBytes(file.FullName);
        }

        public IEnumerable<string> GetPathsIter(string query)
        {
            foreach (var file in GetFilesIter(query))
                yield return file.FullName;
        }

        public IEnumerable<FileInfo> GetFilesIter(string query)
        {
            var songsDirs = _songsDir.GetDirectories();
            var items = songsDirs.Select(songDir => new
            {
                SongDir = songDir,
                Coef = StringEx.CalcSimilarityCoef(query, songDir.Name)
            }).ToArray();
            Array.Sort(items, (a, b) => b.Coef.CompareTo(a.Coef));

            foreach (var item in items)
            {
                foreach (var file in item.SongDir.GetFiles())
                {
                    if (file.Length < MinSize)
                        continue;
                    if (_imgExtensions.Contains(file.Extension.ToLower()))
                        yield return file;
                }
            }
        }

        public IAsyncEnumerator<BitmapImage> GetAsyncEnumerator(string query)
        {
            return new OsuImagesAsyncEnumerator(_songsDir, query, MinSize);
        }

    }
}
