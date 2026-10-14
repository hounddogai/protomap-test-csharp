namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall override calculation.</summary>
public interface ITownHallOverrideModifier
{
    string Describe();
    double Adjust(TownHallOverride item, double valueValue);
}

public sealed record TownHallOverrideRatingOffsetAdjuster(double Weight) : ITownHallOverrideModifier
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(TownHallOverride item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record FlaggedTownHallOverrideAdjuster(Registry Flags, string Flag, double Percent) : ITownHallOverrideModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentTownHallOverrideAdjuster(double Percent) : ITownHallOverrideModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the town hall override steps in order and keeps a trace of each.</summary>
public sealed class TownHallOverrideEngine(List<ITownHallOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallOverrideEngine DefaultTownHallOverrideEngine(Registry flags)
    {
        return new TownHallOverrideEngine(
            new List<ITownHallOverrideModifier>
            {
                new TownHallOverrideRatingOffsetAdjuster(1.27),
                new FlaggedTownHallOverrideAdjuster(flags, "town-hall-override-strict", -0.5),
                new PercentTownHallOverrideAdjuster(13.2),
            },
            new List<string>(),
            1.2,
            52949.3);
    }

    public double Price(TownHallOverride item)
    {
        var current = item.NetVolume();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<TownHallOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
