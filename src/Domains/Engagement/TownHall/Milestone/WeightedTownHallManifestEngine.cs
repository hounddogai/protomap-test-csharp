namespace Acme.Workforce.Engagement.TownHall.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted town hall manifest calculation.</summary>
public interface IWeightedTownHallManifestModifier
{
    string Describe();
    double Adjust(WeightedTownHallManifest item, double valueValue);
}

public sealed record MinimumWeightedTownHallManifestAdjuster(double Minimum) : IWeightedTownHallManifestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedTownHallManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedWeightedTownHallManifestAdjuster(Registry Flags, string Flag, double Percent) : IWeightedTownHallManifestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedTownHallManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWeightedTownHallManifestAdjuster(double Step) : IWeightedTownHallManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedTownHallManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameWeightedTownHallManifestAdjuster(Dictionary<string, double> Factors) : IWeightedTownHallManifestModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(WeightedTownHallManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record WeightedTownHallManifestMaximumOffsetAdjuster(double Weight) : IWeightedTownHallManifestModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(WeightedTownHallManifest item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

/// <summary>Runs the weighted town hall manifest steps in order and keeps a trace of each.</summary>
public sealed class WeightedTownHallManifestEngine(List<IWeightedTownHallManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedTownHallManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedTownHallManifestEngine DefaultWeightedTownHallManifestEngine(Registry flags)
    {
        return new WeightedTownHallManifestEngine(
            new List<IWeightedTownHallManifestModifier>
            {
                new MinimumWeightedTownHallManifestAdjuster(46.1),
                new FlaggedWeightedTownHallManifestAdjuster(flags, "weighted-town-hall-manifest-seasonal", -4.6),
                new RoundingWeightedTownHallManifestAdjuster(0.05),
                new NameWeightedTownHallManifestAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.99, ["west"] = 0.83 }),
                new WeightedTownHallManifestMaximumOffsetAdjuster(1.99),
            },
            new List<string>(),
            2.2,
            47273.8);
    }

    public double Compute(WeightedTownHallManifest item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedTownHallManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
