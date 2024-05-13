using CS2Nades.Common;

namespace CS2Nades.Console.Extensions;

public static class ThrownNadeExtensions
{
    public static string ToConsoleString(this ThrownNade throwNade)
    {
        var position = throwNade.Nade.Lineup.Position;
        var angle = throwNade.Nade.Lineup.Angle;

        var setPosition = $"setpos {position.X} {position.Y} {position.Z}";
        var setAngle = $"setang {angle.Pitch} {angle.Yaw} {angle.Roll}";

        return $"{setPosition}; {setAngle}";
    }
}