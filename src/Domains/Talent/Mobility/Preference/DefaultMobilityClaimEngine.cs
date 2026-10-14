namespace Acme.Workforce.Talent.Mobility.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default mobility claim calculation.</summary>
public interface IDefaultMobilityClaimModifier
{
    string Describe();
    double Adjust(DefaultMobilityClaim item, double valueValue);
}

public sealed record TeamDefaultMobilityClaimAdjuster(Dictionary<string, double> Factors) : IDefaultMobilityClaimModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(DefaultMobilityClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record DefaultMobilityClaimMaximumOffsetAdjuster(double Weight) : IDefaultMobilityClaimModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(DefaultMobilityClaim item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record FlaggedDefaultMobilityClaimAdjuster(Registry Flags, string Flag, double Percent) : IDefaultMobilityClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultMobilityClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDefaultMobilityClaimAdjuster(double Percent) : IDefaultMobilityClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultMobilityClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default mobility claim steps in order and keeps a trace of each.</summary>
public sealed class DefaultMobilityClaimEngine(List<IDefaultMobilityClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultMobilityClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultMobilityClaimEngine DefaultDefaultMobilityClaimEngine(Registry flags)
    {
        return new DefaultMobilityClaimEngine(
            new List<IDefaultMobilityClaimModifier>
            {
                new TeamDefaultMobilityClaimAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.92, ["retail"] = 0.8 }),
                new DefaultMobilityClaimMaximumOffsetAdjuster(1.48),
                new FlaggedDefaultMobilityClaimAdjuster(flags, "default-mobility-claim-seasonal", -5.9),
                new PercentDefaultMobilityClaimAdjuster(-8.5),
            },
            new List<string>(),
            3.9,
            55914.9);
    }

    public double Compute(DefaultMobilityClaim item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultMobilityClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
