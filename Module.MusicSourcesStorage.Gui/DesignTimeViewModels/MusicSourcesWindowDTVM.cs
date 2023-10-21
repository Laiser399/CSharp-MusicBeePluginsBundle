﻿using Module.MusicSourcesStorage.Gui.AbstractViewModels;
using Module.MusicSourcesStorage.Gui.Enums;

namespace Module.MusicSourcesStorage.Gui.DesignTimeViewModels;

public sealed class MusicSourcesWindowDTVM : IMusicSourcesWindowVM
{
    public IList<IMusicSourceVM> MusicSources { get; }
    public IMusicSourceVM? SelectedMusicSource { get; set; }

    public MusicSourcesWindowDTVM()
    {
        var hundred = new List<INodeVM>();
        for (var i = 0; i < 100; i++)
        {
            hundred.Add(new MusicFileDTVM($"Song{i}.mp3", MusicFileState.NotListened));
        }

        MusicSources = new List<IMusicSourceVM>
        {
            new MusicSourceDTVM("First", MusicSourceType.Torrent, new INodeVM[]
            {
                new DirectoryDTVM("Album 1", new INodeVM[]
                {
                    new MusicFileDTVM("Song1.mp3", MusicFileState.NotListened),
                    new MusicFileDTVM("Song2.mp3", MusicFileState.InIncoming),
                    new MusicFileDTVM("Song999.mp3", MusicFileState.InLibrary),
                    new MusicFileDTVM("Song42.mp3", MusicFileState.ListenedAndDeleted),
                    new ImageFileDTVM("cover.jpg", true)
                }, "quad.png"),
                new DirectoryDTVM("Epic Album 666", new INodeVM[]
                {
                    new DirectoryDTVM("Special", new INodeVM[]
                    {
                        new UnknownFileDTVM("song lyrics.txt"),
                        new UnknownFileDTVM("message from author.txt")
                    }),
                    new MusicFileDTVM("Single.flac", MusicFileState.ListenedAndDeleted),
                    new ImageFileDTVM("cover.png", true),
                    new ImageFileDTVM("some image.png", false)
                }, "vertical.png"),
                new DirectoryDTVM("Just a joke", new INodeVM[]
                {
                    new ImageFileDTVM("only-cover.png", true)
                }, "horizontal.png")
            }),
            new MusicSourceDTVM("2nd", MusicSourceType.VkPost, new INodeVM[]
            {
                new MusicFileDTVM("Hello.mp3", MusicFileState.InLibrary),
                new MusicFileDTVM("There.mp3", MusicFileState.ListenedAndDeleted)
            }),
            new MusicSourceDTVM("So Deep", MusicSourceType.VkPost, new INodeVM[]
            {
                new DirectoryDTVM("Right", new INodeVM[]
                {
                    new DirectoryDTVM("Into", new INodeVM[]
                    {
                        new DirectoryDTVM("The", new INodeVM[]
                        {
                            new DirectoryDTVM("Abyss")
                        })
                    })
                })
            }),
            new MusicSourceDTVM("So BIG", MusicSourceType.Torrent, hundred)
        };

        for (var i = 0; i < 100; i++)
        {
            MusicSources.Add(new MusicSourceDTVM("-Inf", MusicSourceType.Torrent));
        }

        SelectedMusicSource = MusicSources[0];
    }
}