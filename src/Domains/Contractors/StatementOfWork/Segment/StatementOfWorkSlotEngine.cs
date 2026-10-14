namespace Acme.Workforce.Contractors.StatementOfWork.Segment;

/// <summary>One step of the statement of work slot calculation.</summary>
public interface IStatementOfWorkSlotAdjuster
{
    string Describe();
    double Adjust(StatementOfWorkSlot item, double valueValue);
}

public sealed record PercentStatementOfWorkSlotAdjuster(double Percent) : IStatementOfWorkSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStatementOfWorkSlotAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkSlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkSlot item, double valueValue)
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

public sealed record MinimumStatementOfWorkSlotAdjuster(double Minimum) : IStatementOfWorkSlotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StatementOfWorkSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the statement of work slot steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkSlotEngine(List<IStatementOfWorkSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkSlotEngine DefaultStatementOfWorkSlotEngine()
    {
        return new StatementOfWorkSlotEngine(
            new List<IStatementOfWorkSlotAdjuster>
            {
                new PercentStatementOfWorkSlotAdjuster(19.3),
                new TieredStatementOfWorkSlotAdjuster(
                    new List<double> { 4246.0, 5864.0, 7895.0 },
                    new List<double> { 1.12, 0.92, 1.17 }),
                new MinimumStatementOfWorkSlotAdjuster(24.9),
            },
            new List<string>(),
            1.5,
            31222.1);
    }

    public double Run(StatementOfWorkSlot item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
