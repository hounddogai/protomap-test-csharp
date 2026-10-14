namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted balance snapshot calculation.</summary>
public interface IWeightedBalanceSnapshotModifier
{
    string Describe();
    double Adjust(WeightedBalanceSnapshot item, double valueValue);
}

public sealed record CapWeightedBalanceSnapshotAdjuster(double Cap) : IWeightedBalanceSnapshotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedBalanceSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumWeightedBalanceSnapshotAdjuster(double Minimum) : IWeightedBalanceSnapshotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedBalanceSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredWeightedBalanceSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IWeightedBalanceSnapshotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedBalanceSnapshot item, double valueValue)
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

public sealed record FlaggedWeightedBalanceSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IWeightedBalanceSnapshotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedBalanceSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWeightedBalanceSnapshotAdjuster(double Step) : IWeightedBalanceSnapshotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedBalanceSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the weighted balance snapshot steps in order and keeps a trace of each.</summary>
public sealed class WeightedBalanceSnapshotEngine(List<IWeightedBalanceSnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedBalanceSnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedBalanceSnapshotEngine DefaultWeightedBalanceSnapshotEngine(Registry flags)
    {
        return new WeightedBalanceSnapshotEngine(
            new List<IWeightedBalanceSnapshotModifier>
            {
                new CapWeightedBalanceSnapshotAdjuster(10347.0),
                new MinimumWeightedBalanceSnapshotAdjuster(48.7),
                new TieredWeightedBalanceSnapshotAdjuster(
                    new List<double> { 5203.0, 6948.0, 9545.0 },
                    new List<double> { 1.09, 0.9, 1.05 }),
                new FlaggedWeightedBalanceSnapshotAdjuster(flags, "weighted-balance-snapshot-scheduled", 3.2),
                new RoundingWeightedBalanceSnapshotAdjuster(0.25),
            },
            new List<string>(),
            2.2,
            8445.0);
    }

    public double Compute(WeightedBalanceSnapshot item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedBalanceSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
