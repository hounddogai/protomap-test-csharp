namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work summary calculation.</summary>
public interface IStatementOfWorkSummaryFactor
{
    string Describe();
    double Adjust(StatementOfWorkSummary item, double valueValue);
}

public sealed record CapStatementOfWorkSummaryAdjuster(double Cap) : IStatementOfWorkSummaryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record StatementOfWorkSummaryQuantityOffsetAdjuster(double Weight) : IStatementOfWorkSummaryFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(StatementOfWorkSummary item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record FlaggedStatementOfWorkSummaryAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkSummaryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStatementOfWorkSummaryAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkSummaryFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkSummary item, double valueValue)
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

public sealed record RoundingStatementOfWorkSummaryAdjuster(double Step) : IStatementOfWorkSummaryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the statement of work summary steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkSummaryEngine(List<IStatementOfWorkSummaryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkSummaryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkSummaryEngine DefaultStatementOfWorkSummaryEngine(Registry flags)
    {
        return new StatementOfWorkSummaryEngine(
            new List<IStatementOfWorkSummaryFactor>
            {
                new CapStatementOfWorkSummaryAdjuster(13940.0),
                new StatementOfWorkSummaryQuantityOffsetAdjuster(1.56),
                new FlaggedStatementOfWorkSummaryAdjuster(flags, "statement-of-work-summary-weighted", -2.7),
                new TieredStatementOfWorkSummaryAdjuster(
                    new List<double> { 748.0, 2841.0, 3066.0, 7032.0 },
                    new List<double> { 0.98, 1.07, 0.99, 0.95 }),
                new RoundingStatementOfWorkSummaryAdjuster(0.05),
            },
            new List<string>(),
            1.6,
            33173.7);
    }

    public double Compute(StatementOfWorkSummary item)
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
