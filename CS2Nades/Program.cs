using System.Text.Json;
using System.Threading.Channels;
using DemoFile;
using DemoFile.Sdk;
using CS2Nades.JsonConverters;

var pathArgument = args.SingleOrDefault();

if (string.IsNullOrEmpty(pathArgument))
{
    Console.Error.WriteLine("Wrong argument: path to a demo file.");
    Environment.Exit(1);
}

var path = pathArgument.Normalize();

if (!File.Exists(path))
{
    Console.Error.WriteLine($"File doesn't exist: {path}.");
    Environment.Exit(1);
}

var results = Channel.CreateUnbounded<Result>();
var demo = new DemoParser();

var projectileTemporalData = new Dictionary<CBaseCSGrenadeProjectile, (ThrowLineup, Timing)>();

demo.EntityEvents.CBaseCSGrenadeProjectile.Create += e =>
{
    var pawn = e.Thrower;

    if (pawn == null) return;

    var lineup = new ThrowLineup(pawn.Origin, pawn.EyeAngles, pawn.InputButtons);

    projectileTemporalData.TryAdd(e, (lineup, new(demo.CurrentGameTick, demo.CurrentGameTime)));
};

demo.EntityEvents.CBaseCSGrenadeProjectile.Delete += e =>
{
    var gotData = projectileTemporalData.TryGetValue(e, out var data);

    if (!gotData) return;

    var (lineup, throwTiming) = data;

    projectileTemporalData.Remove(e);

    var grenadeName = e.GetType().Name.TrimStart('C').Replace("Projectile", string.Empty);

    var nade = new Nade(
        grenadeName,
        lineup,
        e.Origin);

    var expireTiming = new Timing(demo.CurrentGameTick, demo.CurrentGameTime);

    var timings = new Timings(throwTiming!, expireTiming);

    var throwerName = e.Thrower?.Controller?.PlayerName;
    var throwerId = e.Thrower?.Controller?.SteamID;

    var thrower = new Thrower(throwerName, throwerId);

    var throwerPlace = e.Thrower?.LastPlaceName;

    var pos = nade.Lineup.Origin;
    var ang = nade.Lineup.Angle;

    var setpos = $"setpos {pos.X} {pos.Y} {pos.Z}";
    var setang = $"setang {ang.Pitch} {ang.Yaw} {ang.Roll}";

    var console = $"{setpos}; {setang}";

    results.Writer.TryWrite(new(
        Thrower: thrower,
        From: throwerPlace,
        Timings: timings,
        Nade: nade,
        Console: console));
};

Task demoReadTask = null!;

try
{
    demoReadTask = demo.ReadAllAsync(File.OpenRead(path)).AsTask();
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.Exit(1);
}

var _ = demoReadTask.ContinueWith((_) => results.Writer.Complete());

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

jsonOptions.Converters.Add(new InputButtonsJsonConverter());
jsonOptions.Converters.Add(new GameTickJsonConverter());
jsonOptions.Converters.Add(new GameTimeJsonConverter());

await foreach (var nade in results.Reader.ReadAllAsync())
{
    Console.Out.WriteLine(JsonSerializer.Serialize(nade, jsonOptions));
}

record ThrowLineup(Vector Origin, QAngle Angle, InputButtons Buttons);
record Nade(string Name, ThrowLineup Lineup, Vector Origin);
record Timing(GameTick Tick, GameTime Time);
record Timings(Timing Throw, Timing Expire);
record Thrower(string? Name, ulong? SteamId);
record Result(Nade Nade, Thrower Thrower, string? From, Timings Timings, string Console);
