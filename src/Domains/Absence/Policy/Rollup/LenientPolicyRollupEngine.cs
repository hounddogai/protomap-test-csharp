namespace Acme.Workforce.Absence.Policy.Rollup;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the lenient policy rollup calculation.</summary>
public interface ILenientPolicyRollupStep
{
    string Describe();
    double Adjust(LenientPolicyRollup item, double valueValue);
}

public sealed record CapLenientPolicyRollupAdjuster(double Cap) : ILenientPolicyRollupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientPolicyRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLenientPolicyRollupAdjuster(double Percent) : ILenientPolicyRollupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientPolicyRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientPolicyRollupAdjuster(List<double> Bounds, List<double> Rates) : ILenientPolicyRollupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientPolicyRollup item, double valueValue)
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

public sealed record LenientPolicyRollupRevisionOffsetAdjuster(double Weight) : ILenientPolicyRollupStep
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(LenientPolicyRollup item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record FlaggedLenientPolicyRollupAdjuster(Registry Flags, string Flag, double Percent) : ILenientPolicyRollupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientPolicyRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient policy rollup steps in order and keeps a trace of each.</summary>
public sealed class LenientPolicyRollupEngine(List<ILenientPolicyRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientPolicyRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientPolicyRollupEngine DefaultLenientPolicyRollupEngine(Registry flags)
    {
        return new LenientPolicyRollupEngine(
            new List<ILenientPolicyRollupStep>
            {
                new CapLenientPolicyRollupAdjuster(14142.0),
                new PercentLenientPolicyRollupAdjuster(12.2),
                new TieredLenientPolicyRollupAdjuster(
                    new List<double> { 5039.0, 5245.0, 7182.0, 9834.0 },
                    new List<double> { 0.96, 0.8, 0.82, 0.92 }),
                new LenientPolicyRollupRevisionOffsetAdjuster(0.51),
                new FlaggedLenientPolicyRollupAdjuster(flags, "lenient-policy-rollup-regional", 0.3),
            },
            new List<string>(),
            2.4,
            3662.0);
    }

    public double Price(LenientPolicyRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientPolicyRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
