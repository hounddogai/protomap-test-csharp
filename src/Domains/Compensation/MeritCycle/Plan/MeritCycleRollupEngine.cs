namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle rollup calculation.</summary>
public interface IMeritCycleRollupStep
{
    string Describe();
    double Adjust(MeritCycleRollup item, double valueValue);
}

public sealed record CapMeritCycleRollupAdjuster(double Cap) : IMeritCycleRollupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCycleRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ChannelMeritCycleRollupAdjuster(Dictionary<string, double> Factors) : IMeritCycleRollupStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(MeritCycleRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumMeritCycleRollupAdjuster(double Minimum) : IMeritCycleRollupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCycleRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentMeritCycleRollupAdjuster(double Percent) : IMeritCycleRollupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredMeritCycleRollupAdjuster(List<double> Bounds, List<double> Rates) : IMeritCycleRollupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCycleRollup item, double valueValue)
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

/// <summary>Runs the merit cycle rollup steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleRollupEngine(List<IMeritCycleRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleRollupEngine DefaultMeritCycleRollupEngine()
    {
        return new MeritCycleRollupEngine(
            new List<IMeritCycleRollupStep>
            {
                new CapMeritCycleRollupAdjuster(38290.0),
                new ChannelMeritCycleRollupAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.03, ["west"] = 1.19, ["retail"] = 1.05 }),
                new MinimumMeritCycleRollupAdjuster(8.8),
                new PercentMeritCycleRollupAdjuster(-10.8),
                new TieredMeritCycleRollupAdjuster(
                    new List<double> { 2639.0, 8203.0, 8531.0 },
                    new List<double> { 0.83, 0.83, 0.85 }),
            },
            new List<string>(),
            3.5,
            22521.3);
    }

    public double Run(MeritCycleRollup item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
