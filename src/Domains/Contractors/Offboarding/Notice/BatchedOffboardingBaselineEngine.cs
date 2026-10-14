namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched offboarding baseline calculation.</summary>
public interface IBatchedOffboardingBaselineAdjuster
{
    string Describe();
    double Adjust(BatchedOffboardingBaseline item, double valueValue);
}

public sealed record CapBatchedOffboardingBaselineAdjuster(double Cap) : IBatchedOffboardingBaselineAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedOffboardingBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentBatchedOffboardingBaselineAdjuster(double Percent) : IBatchedOffboardingBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedOffboardingBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBatchedOffboardingBaselineAdjuster(Registry Flags, string Flag, double Percent) : IBatchedOffboardingBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedOffboardingBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched offboarding baseline steps in order and keeps a trace of each.</summary>
public sealed class BatchedOffboardingBaselineEngine(List<IBatchedOffboardingBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedOffboardingBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedOffboardingBaselineEngine DefaultBatchedOffboardingBaselineEngine(Registry flags)
    {
        return new BatchedOffboardingBaselineEngine(
            new List<IBatchedOffboardingBaselineAdjuster>
            {
                new CapBatchedOffboardingBaselineAdjuster(30311.0),
                new PercentBatchedOffboardingBaselineAdjuster(-12.4),
                new FlaggedBatchedOffboardingBaselineAdjuster(flags, "batched-offboarding-baseline-capped", -9.1),
            },
            new List<string>(),
            0.9,
            1007.8);
    }

    public double Compute(BatchedOffboardingBaseline item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedOffboardingBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
