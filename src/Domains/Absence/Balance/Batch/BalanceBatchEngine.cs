namespace Acme.Workforce.Absence.Balance.Batch;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the balance batch calculation.</summary>
public interface IBalanceBatchStep
{
    string Describe();
    double Adjust(BalanceBatch item, double valueValue);
}

public sealed record PercentBalanceBatchAdjuster(double Percent) : IBalanceBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BalanceBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamBalanceBatchAdjuster(Dictionary<string, double> Factors) : IBalanceBatchStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(BalanceBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBalanceBatchAdjuster(Registry Flags, string Flag, double Percent) : IBalanceBatchStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BalanceBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the balance batch steps in order and keeps a trace of each.</summary>
public sealed class BalanceBatchEngine(List<IBalanceBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceBatchEngine DefaultBalanceBatchEngine(Registry flags)
    {
        return new BalanceBatchEngine(
            new List<IBalanceBatchStep>
            {
                new PercentBalanceBatchAdjuster(-18.2),
                new TeamBalanceBatchAdjuster(new Dictionary<string, double> { ["west"] = 0.82, ["online"] = 0.84 }),
                new FlaggedBalanceBatchAdjuster(flags, "balance-batch-capped", 7.3),
            },
            new List<string>(),
            3.7,
            26827.0);
    }

    public double Compute(BalanceBatch item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
