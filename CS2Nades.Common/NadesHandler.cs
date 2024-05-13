using System.Text.Json;
using DemoFile;
using DemoFile.Sdk;

namespace CS2Nades.Common;

public class NadesHandler
{
    private readonly Dictionary<CCSPlayerPawn, ThrowLineup> lastGrenadeOwnerLineup = [];
    private readonly Dictionary<CBaseCSGrenadeProjectile, (ThrowLineup, Timing)> projectileTemporalData = [];

    public Action<Result>? OnResult;

    public NadesHandler(DemoParser demoParser)
    {
        demoParser.EntityEvents.CBaseCSGrenade.PostUpdate += e =>
        {
            if (e.NextHoldTick == default) return;

            if (e.OwnerEntity is CCSPlayerPawn pawn)
            {
                lastGrenadeOwnerLineup[pawn] = new ThrowLineup(pawn.Origin, pawn.EyeAngles, pawn.InputButtons);
            }
        };

        demoParser.EntityEvents.CBaseCSGrenadeProjectile.Create += e =>
        {
            var pawn = e.Thrower;

            if (pawn == null) return;

            var lineup = lastGrenadeOwnerLineup.GetValueOrDefault(pawn);

            if (lineup == null) return;

            projectileTemporalData.TryAdd(e, (lineup, new(demoParser.CurrentGameTick, demoParser.CurrentGameTime)));
        };

        demoParser.EntityEvents.CBaseCSGrenadeProjectile.Delete += e =>
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

            var expireTiming = new Timing(demoParser.CurrentGameTick, demoParser.CurrentGameTime);

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

            OnResult?.Invoke(new(
                Thrower: thrower,
                From: throwerPlace,
                Timings: timings,
                Nade: nade,
                Console: console));
        };
    }
}

public record ThrowLineup(Vector Origin, QAngle Angle, InputButtons Buttons);
public record Nade(string Name, ThrowLineup Lineup, Vector Origin);
public record Timing(GameTick Tick, GameTime Time);
public record Timings(Timing Throw, Timing Expire);
public record Thrower(string? Name, ulong? SteamId);
public record Result(Nade Nade, Thrower Thrower, string? From, Timings Timings, string Console);
