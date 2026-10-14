namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary policy override calculation.</summary>
public interface IPrimaryPolicyOverrideAdjuster
{
    string Describe();
    double Adjust(PrimaryPolicyOverride item, double valueValue);
}

public sealed record FlaggedPrimaryPolicyOverrideAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryPolicyOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryPolicyOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPrimaryPolicyOverrideAdjuster(double Percent) : IPrimaryPolicyOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryPolicyOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PrimaryPolicyOverrideCapacityOffsetAdjuster(double Weight) : IPrimaryPolicyOverrideAdjuster
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(PrimaryPolicyOverride item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record TieredPrimaryPolicyOverrideAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryPolicyOverrideAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryPolicyOverride item, double valueValue)
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

public sealed record MinimumPrimaryPolicyOverrideAdjuster(double Minimum) : IPrimaryPolicyOverrideAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryPolicyOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the primary policy override steps in order and keeps a trace of each.</summary>
public sealed class PrimaryPolicyOverrideEngine(List<IPrimaryPolicyOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryPolicyOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryPolicyOverrideEngine DefaultPrimaryPolicyOverrideEngine(Registry flags)
    {
        return new PrimaryPolicyOverrideEngine(
            new List<IPrimaryPolicyOverrideAdjuster>
            {
                new FlaggedPrimaryPolicyOverrideAdjuster(flags, "primary-policy-override-weighted", 5.4),
                new PercentPrimaryPolicyOverrideAdjuster(4.5),
                new PrimaryPolicyOverrideCapacityOffsetAdjuster(0.54),
                new TieredPrimaryPolicyOverrideAdjuster(
                    new List<double> { 1096.0, 3194.0, 3334.0, 6453.0 },
                    new List<double> { 1.08, 0.81, 1.09, 1.04 }),
                new MinimumPrimaryPolicyOverrideAdjuster(28.8),
            },
            new List<string>(),
            1.5,
            3156.7);
    }

    public double Evaluate(PrimaryPolicyOverride item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryPolicyOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
