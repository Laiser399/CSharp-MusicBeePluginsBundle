﻿using MusicBeePlugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VkMusicDownloader.Ex;

namespace VkMusicDownloader.GUI
{
    public class AddingIncomingVM : BaseViewModel
    {
        #region Bindings

        private RelayCommand _refreshCmd;
        public RelayCommand RefreshCmd
            => _refreshCmd ?? (_refreshCmd = new RelayCommand(_ => Refresh()));

        private ObservableCollection<IncomingAudioVM> _incomingAudios;
        public ObservableCollection<IncomingAudioVM> IncomingAudios
            => _incomingAudios ?? (_incomingAudios = new ObservableCollection<IncomingAudioVM>());

        private ObservableCollection<MBAudioVM> _lastMBAudios;
        public ObservableCollection<MBAudioVM> LastMBAudios
            => _lastMBAudios ?? (_lastMBAudios = new ObservableCollection<MBAudioVM>());

        private RelayCommand _addToMBLibraryCmd;
        public RelayCommand AddToMBLibraryCmd
            => _addToMBLibraryCmd ?? (_addToMBLibraryCmd = new RelayCommand(arg =>
            {
                if (arg is IncomingAudioVM incomingAudio)
                    AddToMBLibrary(incomingAudio);
            }));

        #endregion

        private readonly Plugin.MusicBeeApiInterface _mbApi;
        
        private int _prevIndex = -1;

        public AddingIncomingVM(Plugin.MusicBeeApiInterface mbApi)
        {
            _mbApi = mbApi;
        }
        
        private void AddToMBLibrary(IncomingAudioVM incomingAudio)
        {
            _prevIndex += 1;
            int currentIndex = _prevIndex;

            Plugin.CalcIndices(currentIndex, out int i1, out int i2);
            _mbApi.Library_AddFileToLibrary(incomingAudio.FilePath, Plugin.LibraryCategory.Music);
            _mbApi.SetIndex(incomingAudio.FilePath, currentIndex, false);
            _mbApi.SetIndex1(incomingAudio.FilePath, i1, false);
            _mbApi.SetIndex2(incomingAudio.FilePath, i2, false);
            _mbApi.Library_SetFileTag(incomingAudio.FilePath, Plugin.MetaDataType.Artist, incomingAudio.Artist);
            _mbApi.Library_SetFileTag(incomingAudio.FilePath, Plugin.MetaDataType.TrackTitle, incomingAudio.Title);
            _mbApi.Library_CommitTagsToFile(incomingAudio.FilePath);

            Refresh();
        }

        private void Refresh()
        {
            IncomingAudios.Clear();
            LastMBAudios.Clear();

            var incomingAudios = GetIncomingAudios();
            var lastMBAudios = GetLastMBAudios();

            if (lastMBAudios.Length > 0)
                _prevIndex = lastMBAudios[0].Index;
            else
                _prevIndex = -1;

            IncomingAudios.AddRange(incomingAudios);
            LastMBAudios.AddRange(lastMBAudios);
        }

        private IReadOnlyCollection<IncomingAudioVM> GetIncomingAudios()
        {
            var query = $"<Source Type=\"4\"></Source>";
            _mbApi.Library_QueryFilesEx(query, out var paths);

            return paths
                .Select(path => new IncomingAudioVM()
                {
                    FilePath = path,
                    Artist = _mbApi.Library_GetFileTag(path, Plugin.MetaDataType.Artist),
                    Title = _mbApi.Library_GetFileTag(path, Plugin.MetaDataType.TrackTitle)
                })
                .ToList()
                .AsReadOnly();
        }

        private MBAudioVM[] GetLastMBAudios()
        {
            if (!_mbApi.Library_QueryFilesEx("", out string[] paths))
                return Array.Empty<MBAudioVM>();

            var list = paths.Select(path =>
            {
                if (!_mbApi.TryGetIndex(path, out int index))
                    return null;
                if (!_mbApi.TryGetVkId(path, out long vkId))
                    vkId = -1;

                return new
                {
                    Index = index,
                    VkId = vkId,
                    Path = path
                };
            })
            .Where(item => item is object)
            .ToList();

            list.Sort((a, b) => b.Index.CompareTo(a.Index));

            int countOfLast = 3;
            if (list.Count > countOfLast)
                list.RemoveRange(countOfLast, list.Count - countOfLast);

            return list.Select(item => new MBAudioVM()
            {
                Artist = _mbApi.Library_GetFileTag(item.Path, Plugin.MetaDataType.Artist),
                Title = _mbApi.Library_GetFileTag(item.Path, Plugin.MetaDataType.TrackTitle),
                Index = item.Index,
                VkId = item.VkId
            }).ToArray();
        }

        
    }
}
