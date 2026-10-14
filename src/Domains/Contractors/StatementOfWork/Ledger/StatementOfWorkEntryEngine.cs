namespace Acme.Workforce.Contractors.StatementOfWork.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work entry calculation.</summary>
public interface IStatementOfWorkEntryFactor
{
    string Describe();
    double Adjust(StatementOfWorkEntry item, double valueValue);
}

public sealed record PercentStatementOfWorkEntryAdjuster(double Percent) : IStatementOfWorkEntryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumStatementOfWorkEntryAdjuster(double Minimum) : IStatementOfWorkEntryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StatementOfWorkEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredStatementOfWorkEntryAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkEntryFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkEntry item, double valueValue)
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

public sealed record CapStatementOfWorkEntryAdjuster(double Cap) : IStatementOfWorkEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the statement of work entry steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkEntryEngine(List<IStatementOfWorkEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkEntryEngine DefaultStatementOfWorkEntryEngine()
    {
        return new StatementOfWorkEntryEngine(
            new List<IStatementOfWorkEntryFactor>
            {
                new PercentStatementOfWorkEntryAdjuster(5.7),
                new MinimumStatementOfWorkEntryAdjuster(25.1),
                new TieredStatementOfWorkEntryAdjuster(
                    new List<double> { 2450.0, 7310.0, 7803.0, 7856.0 },
                    new List<double> { 0.82, 0.99, 0.92, 0.82 }),
                new CapStatementOfWorkEntryAdjuster(12919.0),
            },
            new List<string>(),
            2.4,
            14895.8);
    }

    public double Price(StatementOfWorkEntry item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
