namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>One step of the statement of work metric calculation.</summary>
public interface IStatementOfWorkMetricFactor
{
    string Describe();
    double Adjust(StatementOfWorkMetric item, double valueValue);
}

public sealed record CapStatementOfWorkMetricAdjuster(double Cap) : IStatementOfWorkMetricFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamStatementOfWorkMetricAdjuster(Dictionary<string, double> Factors) : IStatementOfWorkMetricFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(StatementOfWorkMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentStatementOfWorkMetricAdjuster(double Percent) : IStatementOfWorkMetricFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record StatementOfWorkMetricVersionOffsetAdjuster(double Weight) : IStatementOfWorkMetricFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(StatementOfWorkMetric item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record TieredStatementOfWorkMetricAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkMetricFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkMetric item, double valueValue)
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

/// <summary>Runs the statement of work metric steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkMetricEngine(List<IStatementOfWorkMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkMetricEngine DefaultStatementOfWorkMetricEngine()
    {
        return new StatementOfWorkMetricEngine(
            new List<IStatementOfWorkMetricFactor>
            {
                new CapStatementOfWorkMetricAdjuster(19160.0),
                new TeamStatementOfWorkMetricAdjuster(new Dictionary<string, double> { ["south"] = 1.35 }),
                new PercentStatementOfWorkMetricAdjuster(-5.4),
                new StatementOfWorkMetricVersionOffsetAdjuster(0.37),
                new TieredStatementOfWorkMetricAdjuster(
                    new List<double> { 355.0, 4604.0, 8202.0, 9925.0 },
                    new List<double> { 1.07, 1.16, 0.82, 0.83 }),
            },
            new List<string>(),
            4.6,
            10241.5);
    }

    public double Price(StatementOfWorkMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
