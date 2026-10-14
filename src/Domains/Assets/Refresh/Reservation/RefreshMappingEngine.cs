namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh mapping calculation.</summary>
public interface IRefreshMappingAdjuster
{
    string Describe();
    double Adjust(RefreshMapping item, double valueValue);
}

public sealed record LabelRefreshMappingAdjuster(Dictionary<string, double> Factors) : IRefreshMappingAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RefreshMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RefreshMappingBatchSizeOffsetAdjuster(double Weight) : IRefreshMappingAdjuster
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(RefreshMapping item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record RoundingRefreshMappingAdjuster(double Step) : IRefreshMappingAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedRefreshMappingAdjuster(Registry Flags, string Flag, double Percent) : IRefreshMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRefreshMappingAdjuster(List<double> Bounds, List<double> Rates) : IRefreshMappingAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshMapping item, double valueValue)
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

public sealed record MinimumRefreshMappingAdjuster(double Minimum) : IRefreshMappingAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the refresh mapping steps in order and keeps a trace of each.</summary>
public sealed class RefreshMappingEngine(List<IRefreshMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshMappingEngine DefaultRefreshMappingEngine(Registry flags)
    {
        return new RefreshMappingEngine(
            new List<IRefreshMappingAdjuster>
            {
                new LabelRefreshMappingAdjuster(new Dictionary<string, double> { ["south"] = 1.32 }),
                new RefreshMappingBatchSizeOffsetAdjuster(0.44),
                new RoundingRefreshMappingAdjuster(0.5),
                new FlaggedRefreshMappingAdjuster(flags, "refresh-mapping-rolling", 2.9),
                new TieredRefreshMappingAdjuster(
                    new List<double> { 607.0, 1045.0, 2713.0 },
                    new List<double> { 1.07, 0.89, 1.03 }),
                new MinimumRefreshMappingAdjuster(7.8),
            },
            new List<string>(),
            0.5,
            63525.6);
    }

    public double Run(RefreshMapping item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
