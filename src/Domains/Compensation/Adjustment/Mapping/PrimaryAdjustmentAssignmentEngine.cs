namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary adjustment assignment calculation.</summary>
public interface IPrimaryAdjustmentAssignmentStep
{
    string Describe();
    double Adjust(PrimaryAdjustmentAssignment item, double valueValue);
}

public sealed record TieredPrimaryAdjustmentAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryAdjustmentAssignmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryAdjustmentAssignment item, double valueValue)
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

public sealed record MinimumPrimaryAdjustmentAssignmentAdjuster(double Minimum) : IPrimaryAdjustmentAssignmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryAdjustmentAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitlePrimaryAdjustmentAssignmentAdjuster(Dictionary<string, double> Factors) : IPrimaryAdjustmentAssignmentStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PrimaryAdjustmentAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPrimaryAdjustmentAssignmentAdjuster(double Cap) : IPrimaryAdjustmentAssignmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryAdjustmentAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPrimaryAdjustmentAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryAdjustmentAssignmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryAdjustmentAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary adjustment assignment steps in order and keeps a trace of each.</summary>
public sealed class PrimaryAdjustmentAssignmentEngine(List<IPrimaryAdjustmentAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryAdjustmentAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryAdjustmentAssignmentEngine DefaultPrimaryAdjustmentAssignmentEngine(Registry flags)
    {
        return new PrimaryAdjustmentAssignmentEngine(
            new List<IPrimaryAdjustmentAssignmentStep>
            {
                new TieredPrimaryAdjustmentAssignmentAdjuster(
                    new List<double> { 213.0, 3202.0, 5017.0, 9704.0 },
                    new List<double> { 0.95, 1.09, 0.98, 0.88 }),
                new MinimumPrimaryAdjustmentAssignmentAdjuster(3.6),
                new TitlePrimaryAdjustmentAssignmentAdjuster(new Dictionary<string, double> { ["central"] = 1.22 }),
                new CapPrimaryAdjustmentAssignmentAdjuster(2340.0),
                new FlaggedPrimaryAdjustmentAssignmentAdjuster(flags, "primary-adjustment-assignment-batched", 3.3),
            },
            new List<string>(),
            2.7,
            98144.5);
    }

    public double Compute(PrimaryAdjustmentAssignment item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryAdjustmentAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
