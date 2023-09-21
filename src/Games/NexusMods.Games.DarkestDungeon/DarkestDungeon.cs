using System.Diagnostics.CodeAnalysis;
using NexusMods.Common;
using NexusMods.DataModel.Games;
using NexusMods.DataModel.ModInstallers;
using NexusMods.FileExtractor.StreamFactories;
using NexusMods.Games.DarkestDungeon.Installers;
using NexusMods.Paths;
using OneOf.Types;

namespace NexusMods.Games.DarkestDungeon;

public class DarkestDungeon : AGame, ISteamGame, IGogGame, IEpicGame
{
    private readonly IOSInformation _osInformation;

    public IEnumerable<uint> SteamIds => new[] { 262060u };
    public IEnumerable<long> GogIds => new long[] { 1450711444 };
    public IEnumerable<string> EpicCatalogItemId => new[] { "b4eecf70e3fe4e928b78df7855a3fc2d" };

    // TODO: Xbox ID

    public DarkestDungeon(
        IOSInformation osInformation,
        IEnumerable<IGameLocator> gameLocators) : base(gameLocators)
    {
        _osInformation = osInformation;
    }

    public override string Name => "Darkest Dungeon";
    public override GameDomain Domain => GameDomain.From("darkestdungeon");

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public override GamePath GetPrimaryFile(GameStore store)
    {
        return _osInformation.MatchPlatform(
            ref store,
            onWindows: (ref GameStore gameStore) => gameStore == GameStore.Steam
                ? new GamePath(GameFolderType.Game, "_windows/Darkest.exe")
                : new GamePath(GameFolderType.Game, "_windowsnosteam/Darkest.exe"),
            onLinux: (ref GameStore gameStore) => gameStore == GameStore.Steam
                ? new GamePath(GameFolderType.Game, "_linux/darkest.bin.x86_64")
                : new GamePath(GameFolderType.Game, "linuxnosteam/darkest.bin.x86_64"),
            onOSX: (ref GameStore gameStore) => gameStore == GameStore.Steam
                ? new GamePath(GameFolderType.Game, "_osx/Darkest.app/Contents/MacOS/Darkest")
                : new GamePath(GameFolderType.Game, "_osxnosteam/Darkest.app/Contents/MacOS/Darkest NoSteam")
        );
    }

    protected override IReadOnlyDictionary<GameFolderType, AbsolutePath> GetLocations(IFileSystem fileSystem,
        IGameLocator locator,
        GameLocatorResult installation)
    {
        var globalSettingsFile = fileSystem
            .GetKnownPath(KnownPath.LocalApplicationDataDirectory)
            .Combine("Red Hook Studios/Darkest/persist.options.json");

        var result =  new Dictionary<GameFolderType, AbsolutePath>()
        {
            { GameFolderType.Game, installation.Path },
            { GameFolderType.Preferences, globalSettingsFile }
        };

        if (installation.Metadata is SteamLocatorResultMetadata { CloudSavesDirectory: not null } steamLocatorResultMetadata)
        {
            result[GameFolderType.Saves] = steamLocatorResultMetadata.CloudSavesDirectory.Value;
        }
        else
        {
            var savesDirectory = fileSystem
                .GetKnownPath(KnownPath.MyDocumentsDirectory)
                .Combine("Darkest");

            result[GameFolderType.Saves] = savesDirectory;
        }

        return result;
    }

    public override IStreamFactory Icon =>
        new EmbededResourceStreamFactory<DarkestDungeon>("NexusMods.Games.DarkestDungeon.Resources.DarkestDungeon.icon.png");

    public override IStreamFactory GameImage =>
        new EmbededResourceStreamFactory<DarkestDungeon>("NexusMods.Games.DarkestDungeon.Resources.DarkestDungeon.game_image.jpg");


    /// <inheritdoc />
    public override IEnumerable<IModInstaller> Installers => new IModInstaller[]
    {
        new NativeModInstaller(),
        new LooseFilesModInstaller(),
    };
}
