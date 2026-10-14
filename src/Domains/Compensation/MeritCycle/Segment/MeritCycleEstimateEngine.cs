namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle estimate calculation.</summary>
public interface IMeritCycleEstimateModifier
{
    string Describe();
    double Adjust(MeritCycleEstimate item, double valueValue);
}

public sealed record CapMeritCycleEstimateAdjuster(double Cap) : IMeritCycleEstimateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCycleEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentMeritCycleEstimateAdjuster(double Percent) : IMeritCycleEstimateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedMeritCycleEstimateAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleEstimateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelMeritCycleEstimateAdjuster(Dictionary<string, double> Factors) : IMeritCycleEstimateModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(MeritCycleEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredMeritCycleEstimateAdjuster(List<double> Bounds, List<double> Rates) : IMeritCycleEstimateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCycleEstimate item, double valueValue)
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

/// <summary>Runs the merit cycle estimate steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleEstimateEngine(List<IMeritCycleEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleEstimateEngine DefaultMeritCycleEstimateEngine(Registry flags)
    {
        return new MeritCycleEstimateEngine(
            new List<IMeritCycleEstimateModifier>
            {
                new CapMeritCycleEstimateAdjuster(43668.0),
                new PercentMeritCycleEstimateAdjuster(-7.2),
                new FlaggedMeritCycleEstimateAdjuster(flags, "merit-cycle-estimate-rolling", 2.5),
                new ChannelMeritCycleEstimateAdjuster(new Dictionary<string, double> { ["east"] = 0.83 }),
                new TieredMeritCycleEstimateAdjuster(
                    new List<double> { 174.0, 1400.0, 1574.0, 6414.0 },
                    new List<double> { 1.16, 1.01, 1.01, 1.06 }),
            },
            new List<string>(),
            1.7,
            24138.1);
    }

    public double Price(MeritCycleEstimate item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
