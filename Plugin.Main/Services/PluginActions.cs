﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Module.ArtworksSearcher.Factories;
using Module.AudioSourcesComparer.GUI.Factories;
using Module.DataExporter.Exceptions;
using Module.DataExporter.Services;
using Module.InboxAdder.Services;
using Module.PlaylistsExporter.Services;
using Module.VkAudioDownloader.GUI.Factories;
using Root.Helpers;
using Root.MusicBeeApi;
using Root.MusicBeeApi.Abstract;
using Root.Services.Abstract;

namespace MusicBeePlugin.Services
{
    public class PluginActions : IPluginActions
    {
        private readonly IMusicBeeApi _mbApi;
        private readonly IDataExportService _dataExportService;
        private readonly IPlaylistsExportService _playlistsExportService;
        private readonly IInboxAddService _inboxAddService;
        private readonly ISearchWindowFactory _searchWindowFactory;
        private readonly IVkAudioDownloaderWindowFactory _vkAudioDownloaderWindowFactory;
        private readonly IVkToLocalComparerWindowFactory _vkToLocalComparerWindowFactory;
        private readonly IVkApiAuthorizationsService _vkApiAuthorizationsService;

        public PluginActions(
            IMusicBeeApi mbApi,
            IDataExportService dataExportService,
            IPlaylistsExportService playlistsExportService,
            IInboxAddService inboxAddService,
            ISearchWindowFactory searchWindowFactory,
            IVkAudioDownloaderWindowFactory vkAudioDownloaderWindowFactory,
            IVkToLocalComparerWindowFactory vkToLocalComparerWindowFactory,
            IVkApiAuthorizationsService vkApiAuthorizationsService)
        {
            _mbApi = mbApi;
            _dataExportService = dataExportService;
            _playlistsExportService = playlistsExportService;
            _inboxAddService = inboxAddService;
            _searchWindowFactory = searchWindowFactory;
            _vkAudioDownloaderWindowFactory = vkAudioDownloaderWindowFactory;
            _vkToLocalComparerWindowFactory = vkToLocalComparerWindowFactory;
            _vkApiAuthorizationsService = vkApiAuthorizationsService;
        }

        public void SearchArtworks()
        {
            if (!TryGetSingleSelectedFile(out var selectedFilePath))
            {
                MessageBox.Show("You must select single item.");
                return;
            }

            var artist = _mbApi.Library_GetFileTag(selectedFilePath, MetaDataType.Artist);
            var title = _mbApi.Library_GetFileTag(selectedFilePath, MetaDataType.TrackTitle);

            var searchWindow = _searchWindowFactory.Create();
            if (searchWindow.ShowDialog(artist, title, out var imageData))
            {
                if (!_mbApi.Library_SetArtworkEx(selectedFilePath, 0, imageData))
                {
                    MessageBox.Show("Обложка не была сохранена.", "Ошибка");
                }
            }
        }

        public void DownloadVkAudios()
        {
            var authorized = _vkApiAuthorizationsService.AuthorizeVkApiIfNeeded();
            if (!authorized)
            {
                return;
            }

            _vkAudioDownloaderWindowFactory
                .Create()
                .ShowDialog();
        }

        public void CompareVkAndLocalAudios()
        {
            var authorized = _vkApiAuthorizationsService.AuthorizeVkApiIfNeeded();
            if (!authorized)
            {
                return;
            }

            _vkToLocalComparerWindowFactory
                .Create()
                .Show();
        }

        public void AddSelectedFileToLibrary()
        {
            if (!TryGetSingleSelectedFile(out var selectedFilePath))
            {
                MessageBox.Show("You must select single item.");
                return;
            }

            _inboxAddService.AddToLibrary(selectedFilePath);

            _mbApi.MB_RefreshPanels();
        }

        public void RetrieveSelectedFileToInbox()
        {
            if (!TryGetSingleSelectedFile(out var selectedFilePath))
            {
                MessageBox.Show("You must select single item.");
                return;
            }

            _inboxAddService.RetrieveToInbox(selectedFilePath);

            _mbApi.MB_RefreshPanels();
        }

        public void ExportPlaylists()
        {
            try
            {
                var deletingPlaylistsPaths = _playlistsExportService.GetExistingExportedPlaylists();

                if (!AskForPlaylistsDeletionIfNeeded(deletingPlaylistsPaths))
                {
                    return;
                }

                _playlistsExportService.CleanAndExport();

                MessageBox.Show("Export done successfully.", "(ง ͠° ͟ل͜ ͡°)ง");
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unknown error occured.\n" +
                                $"Exception type: {e.GetType()}\n" +
                                $"Message: {e.Message}",
                    "(╥_╥)");
            }
        }

        public void ExportLibraryData()
        {
            using var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                EnsurePathExists = true
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }

            try
            {
                _dataExportService.Export(dialog.FileName);

                MessageBox.Show("Экспорт выполнен успешно.", "Ок");
            }
            catch (MusicBeeApiException e)
            {
                MessageBox.Show(e.Message, "Error");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Unknown Error");
            }
        }

        private bool TryGetSingleSelectedFile(out string filePath)
        {
            var queryRes = _mbApi.Library_QueryFilesEx("domain=SelectedFiles", out var files);

            if (!queryRes || files.Length != 1)
            {
                filePath = string.Empty;
                return false;
            }

            filePath = files[0];
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deletingPlaylistsPaths"></param>
        /// <returns>true means that you can delete existing playlists and continue export</returns>
        private static bool AskForPlaylistsDeletionIfNeeded(IReadOnlyCollection<string> deletingPlaylistsPaths)
        {
            if (deletingPlaylistsPaths.Count == 0)
            {
                return true;
            }

            const int showCount = 10;

            var deletingPlaylists = deletingPlaylistsPaths
                .Take(showCount)
                .Select(x => new Uri(x))
                .ToReadOnlyList();

            var (common, particulars) = UriHelper.SplitOnCommonAndParticulars(deletingPlaylists);

            var joined = string.Join(Environment.NewLine, particulars.Select(x => $"\t{x.ToLocalOrBackSlashPath()}"));
            var dialogResult = MessageBox.Show("These files will be deleted before export:\n" +
                                               $"{common.ToLocalOrBackSlashPath()}\n" +
                                               $"{joined}\n" +
                                               (deletingPlaylistsPaths.Count > showCount
                                                   ? $"\t{deletingPlaylistsPaths.Count - showCount} more...\n"
                                                   : string.Empty) +
                                               "Continue?",
                "('ʘᗩʘ)",
                MessageBoxButton.OKCancel);

            return dialogResult == MessageBoxResult.OK;
        }
    }
}