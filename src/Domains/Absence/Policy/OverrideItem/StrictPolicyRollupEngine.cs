namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict policy rollup calculation.</summary>
public interface IStrictPolicyRollupStep
{
    string Describe();
    double Adjust(StrictPolicyRollup item, double valueValue);
}

public sealed record FlaggedStrictPolicyRollupAdjuster(Registry Flags, string Flag, double Percent) : IStrictPolicyRollupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictPolicyRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictPolicyRollupAdjuster(List<double> Bounds, List<double> Rates) : IStrictPolicyRollupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictPolicyRollup item, double valueValue)
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

public sealed record StrictPolicyRollupRateOffsetAdjuster(double Weight) : IStrictPolicyRollupStep
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(StrictPolicyRollup item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the strict policy rollup steps in order and keeps a trace of each.</summary>
public sealed class StrictPolicyRollupEngine(List<IStrictPolicyRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPolicyRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPolicyRollupEngine DefaultStrictPolicyRollupEngine(Registry flags)
    {
        return new StrictPolicyRollupEngine(
            new List<IStrictPolicyRollupStep>
            {
                new FlaggedStrictPolicyRollupAdjuster(flags, "strict-policy-rollup-fallback", -1.8),
                new TieredStrictPolicyRollupAdjuster(
                    new List<double> { 5071.0, 6946.0, 8055.0 },
                    new List<double> { 1.09, 1.08, 0.9 }),
                new StrictPolicyRollupRateOffsetAdjuster(0.31),
            },
            new List<string>(),
            2.5,
            13322.5);
    }

    public double Compute(StrictPolicyRollup item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictPolicyRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
