namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the statement of work batch calculation.</summary>
public interface IStatementOfWorkBatchFactor
{
    string Describe();
    double Adjust(StatementOfWorkBatch item, double valueValue);
}

public sealed record CapStatementOfWorkBatchAdjuster(double Cap) : IStatementOfWorkBatchFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentStatementOfWorkBatchAdjuster(double Percent) : IStatementOfWorkBatchFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumStatementOfWorkBatchAdjuster(double Minimum) : IStatementOfWorkBatchFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StatementOfWorkBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredStatementOfWorkBatchAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkBatchFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkBatch item, double valueValue)
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

public sealed record LabelStatementOfWorkBatchAdjuster(Dictionary<string, double> Factors) : IStatementOfWorkBatchFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(StatementOfWorkBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedStatementOfWorkBatchAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkBatchFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the statement of work batch steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkBatchEngine(List<IStatementOfWorkBatchFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkBatchFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkBatchEngine DefaultStatementOfWorkBatchEngine(Registry flags)
    {
        return new StatementOfWorkBatchEngine(
            new List<IStatementOfWorkBatchFactor>
            {
                new CapStatementOfWorkBatchAdjuster(9574.0),
                new PercentStatementOfWorkBatchAdjuster(-12.9),
                new MinimumStatementOfWorkBatchAdjuster(31.0),
                new TieredStatementOfWorkBatchAdjuster(
                    new List<double> { 1161.0, 4377.0, 8094.0 },
                    new List<double> { 0.99, 0.97, 1.16 }),
                new LabelStatementOfWorkBatchAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.79, ["east"] = 1.28, ["south"] = 1.37 }),
                new FlaggedStatementOfWorkBatchAdjuster(flags, "statement-of-work-batch-seasonal", -4.2),
            },
            new List<string>(),
            4.9,
            22408.0);
    }

    public double Evaluate(StatementOfWorkBatch item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
