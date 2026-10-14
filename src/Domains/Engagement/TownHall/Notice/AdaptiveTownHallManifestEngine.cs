namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive town hall manifest calculation.</summary>
public interface IAdaptiveTownHallManifestAdjuster
{
    string Describe();
    double Adjust(AdaptiveTownHallManifest item, double valueValue);
}

public sealed record CapAdaptiveTownHallManifestAdjuster(double Cap) : IAdaptiveTownHallManifestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveTownHallManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAdaptiveTownHallManifestAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveTownHallManifestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveTownHallManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAdaptiveTownHallManifestAdjuster(double Percent) : IAdaptiveTownHallManifestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveTownHallManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdaptiveTownHallManifestMarginOffsetAdjuster(double Weight) : IAdaptiveTownHallManifestAdjuster
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(AdaptiveTownHallManifest item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record TieredAdaptiveTownHallManifestAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveTownHallManifestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveTownHallManifest item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive town hall manifest steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveTownHallManifestEngine(List<IAdaptiveTownHallManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveTownHallManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveTownHallManifestEngine DefaultAdaptiveTownHallManifestEngine(Registry flags)
    {
        return new AdaptiveTownHallManifestEngine(
            new List<IAdaptiveTownHallManifestAdjuster>
            {
                new CapAdaptiveTownHallManifestAdjuster(42453.0),
                new FlaggedAdaptiveTownHallManifestAdjuster(flags, "adaptive-town-hall-manifest-global", 7.9),
                new PercentAdaptiveTownHallManifestAdjuster(-23.6),
                new AdaptiveTownHallManifestMarginOffsetAdjuster(1.61),
                new TieredAdaptiveTownHallManifestAdjuster(
                    new List<double> { 2193.0, 4066.0, 7396.0 },
                    new List<double> { 1.14, 0.83, 0.86 }),
            },
            new List<string>(),
            2.0,
            21739.3);
    }

    public double Evaluate(AdaptiveTownHallManifest item)
    {
        var current = item.NetVolume();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<AdaptiveTownHallManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
