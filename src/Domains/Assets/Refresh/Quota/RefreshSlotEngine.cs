namespace Acme.Workforce.Assets.Refresh.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh slot calculation.</summary>
public interface IRefreshSlotAdjuster
{
    string Describe();
    double Adjust(RefreshSlot item, double valueValue);
}

public sealed record PercentRefreshSlotAdjuster(double Percent) : IRefreshSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRefreshSlotAdjuster(double Minimum) : IRefreshSlotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRefreshSlotAdjuster(List<double> Bounds, List<double> Rates) : IRefreshSlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshSlot item, double valueValue)
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

public sealed record CapRefreshSlotAdjuster(double Cap) : IRefreshSlotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RefreshSlotFteOffsetAdjuster(double Weight) : IRefreshSlotAdjuster
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(RefreshSlot item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record FlaggedRefreshSlotAdjuster(Registry Flags, string Flag, double Percent) : IRefreshSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the refresh slot steps in order and keeps a trace of each.</summary>
public sealed class RefreshSlotEngine(List<IRefreshSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshSlotEngine DefaultRefreshSlotEngine(Registry flags)
    {
        return new RefreshSlotEngine(
            new List<IRefreshSlotAdjuster>
            {
                new PercentRefreshSlotAdjuster(-1.9),
                new MinimumRefreshSlotAdjuster(14.3),
                new TieredRefreshSlotAdjuster(new List<double> { 9166.0, 9470.0 }, new List<double> { 0.87, 0.95 }),
                new CapRefreshSlotAdjuster(6881.0),
                new RefreshSlotFteOffsetAdjuster(1.45),
                new FlaggedRefreshSlotAdjuster(flags, "refresh-slot-manual", 5.0),
            },
            new List<string>(),
            0.2,
            72218.8);
    }

    public double Run(RefreshSlot item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
