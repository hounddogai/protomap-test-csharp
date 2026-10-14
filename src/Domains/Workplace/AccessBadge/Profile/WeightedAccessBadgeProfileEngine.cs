namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted access badge profile calculation.</summary>
public interface IWeightedAccessBadgeProfileAdjuster
{
    string Describe();
    double Adjust(WeightedAccessBadgeProfile item, double valueValue);
}

public sealed record PercentWeightedAccessBadgeProfileAdjuster(double Percent) : IWeightedAccessBadgeProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedAccessBadgeProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWeightedAccessBadgeProfileAdjuster(double Step) : IWeightedAccessBadgeProfileAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedAccessBadgeProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapWeightedAccessBadgeProfileAdjuster(double Cap) : IWeightedAccessBadgeProfileAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedAccessBadgeProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedWeightedAccessBadgeProfileAdjuster(Registry Flags, string Flag, double Percent) : IWeightedAccessBadgeProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedAccessBadgeProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted access badge profile steps in order and keeps a trace of each.</summary>
public sealed class WeightedAccessBadgeProfileEngine(List<IWeightedAccessBadgeProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAccessBadgeProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAccessBadgeProfileEngine DefaultWeightedAccessBadgeProfileEngine(Registry flags)
    {
        return new WeightedAccessBadgeProfileEngine(
            new List<IWeightedAccessBadgeProfileAdjuster>
            {
                new PercentWeightedAccessBadgeProfileAdjuster(-20.9),
                new RoundingWeightedAccessBadgeProfileAdjuster(0.5),
                new CapWeightedAccessBadgeProfileAdjuster(43940.0),
                new FlaggedWeightedAccessBadgeProfileAdjuster(flags, "weighted-access-badge-profile-regional", -7.5),
            },
            new List<string>(),
            3.6,
            40581.3);
    }

    public double Price(WeightedAccessBadgeProfile item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAccessBadgeProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
