namespace Acme.Workforce.Contractors.StatementOfWork.Variance;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the statement of work variance calculation.</summary>
public interface IStatementOfWorkVarianceStep
{
    string Describe();
    double Adjust(StatementOfWorkVariance item, double valueValue);
}

public sealed record FlaggedStatementOfWorkVarianceAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkVarianceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStatementOfWorkVarianceAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkVarianceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkVariance item, double valueValue)
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

public sealed record CapStatementOfWorkVarianceAdjuster(double Cap) : IStatementOfWorkVarianceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentStatementOfWorkVarianceAdjuster(double Percent) : IStatementOfWorkVarianceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the statement of work variance steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkVarianceEngine(List<IStatementOfWorkVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkVarianceEngine DefaultStatementOfWorkVarianceEngine(Registry flags)
    {
        return new StatementOfWorkVarianceEngine(
            new List<IStatementOfWorkVarianceStep>
            {
                new FlaggedStatementOfWorkVarianceAdjuster(flags, "statement-of-work-variance-regional", -2.4),
                new TieredStatementOfWorkVarianceAdjuster(
                    new List<double> { 6207.0, 7530.0 },
                    new List<double> { 0.98, 1.12 }),
                new CapStatementOfWorkVarianceAdjuster(45092.0),
                new PercentStatementOfWorkVarianceAdjuster(-2.1),
            },
            new List<string>(),
            3.0,
            53012.0);
    }

    public double Price(StatementOfWorkVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
