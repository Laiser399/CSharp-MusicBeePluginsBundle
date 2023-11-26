﻿using Module.Core.Helpers;
using Module.MusicSourcesStorage.Core;
using Module.MusicSourcesStorage.Logic.Entities;
using Module.MusicSourcesStorage.Logic.Services.Abstract;

namespace Module.MusicSourcesStorage.Logic.Services;

public sealed class SourceFilesPathService : ISourceFilesPathService
{
    private readonly IModuleConfiguration _configuration;

    public SourceFilesPathService(IModuleConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetSourceFilesRootDirectory(MusicSourceAdditionalInfo additionalInfo)
    {
        return Path.Combine(
            _configuration.SourceFilesDownloadingDirectory,
            GetFixedTargetFilesDirectory(additionalInfo)
        );
    }

    public string GetSourceFileTargetPath(MusicSourceAdditionalInfo additionalInfo, SourceFile file)
    {
        return Path.Combine(
            _configuration.SourceFilesDownloadingDirectory,
            GetFixedTargetFilesDirectory(additionalInfo),
            file.Path
        );
    }

    private static string GetFixedTargetFilesDirectory(MusicSourceAdditionalInfo additionalInfo)
    {
        return PathHelper.ReplaceInvalidChars(additionalInfo.TargetFilesDirectory, "_");
    }
}