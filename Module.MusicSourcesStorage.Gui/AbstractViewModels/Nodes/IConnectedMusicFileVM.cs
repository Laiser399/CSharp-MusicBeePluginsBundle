﻿using System.Windows.Input;
using Module.MusicSourcesStorage.Logic.Enums;

namespace Module.MusicSourcesStorage.Gui.AbstractViewModels.Nodes;

public interface IConnectedMusicFileVM :
    IMusicFileVM,
    IProcessableVM,
    IDownloadableVM,
    IMarkableAsListenedVM,
    IDeletableVM
{
    MusicFileLocation Location { get; }

    ICommand DeleteAndMarkAsListened { get; }
}