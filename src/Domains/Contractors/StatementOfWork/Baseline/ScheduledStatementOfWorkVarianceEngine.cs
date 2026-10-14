namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled statement of work variance calculation.</summary>
public interface IScheduledStatementOfWorkVarianceAdjuster
{
    string Describe();
    double Adjust(ScheduledStatementOfWorkVariance item, double valueValue);
}

public sealed record ScheduledStatementOfWorkVarianceStartsAtOffsetAdjuster(double Weight) : IScheduledStatementOfWorkVarianceAdjuster
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(ScheduledStatementOfWorkVariance item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record FlaggedScheduledStatementOfWorkVarianceAdjuster(Registry Flags, string Flag, double Percent) : IScheduledStatementOfWorkVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledStatementOfWorkVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentScheduledStatementOfWorkVarianceAdjuster(double Percent) : IScheduledStatementOfWorkVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledStatementOfWorkVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScheduledStatementOfWorkVarianceAdjuster(List<double> Bounds, List<double> Rates) : IScheduledStatementOfWorkVarianceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledStatementOfWorkVariance item, double valueValue)
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

public sealed record CapScheduledStatementOfWorkVarianceAdjuster(double Cap) : IScheduledStatementOfWorkVarianceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledStatementOfWorkVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the scheduled statement of work variance steps in order and keeps a trace of each.</summary>
public sealed class ScheduledStatementOfWorkVarianceEngine(List<IScheduledStatementOfWorkVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledStatementOfWorkVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledStatementOfWorkVarianceEngine DefaultScheduledStatementOfWorkVarianceEngine(Registry flags)
    {
        return new ScheduledStatementOfWorkVarianceEngine(
            new List<IScheduledStatementOfWorkVarianceAdjuster>
            {
                new ScheduledStatementOfWorkVarianceStartsAtOffsetAdjuster(1.29),
                new FlaggedScheduledStatementOfWorkVarianceAdjuster(
                    flags,
                    "scheduled-statement-of-work-variance-adaptive",
                    -7.7),
                new PercentScheduledStatementOfWorkVarianceAdjuster(-20.8),
                new TieredScheduledStatementOfWorkVarianceAdjuster(
                    new List<double> { 2434.0, 8167.0, 8930.0 },
                    new List<double> { 1.08, 0.88, 0.81 }),
                new CapScheduledStatementOfWorkVarianceAdjuster(41994.0),
            },
            new List<string>(),
            4.8,
            19060.6);
    }

    public double Run(ScheduledStatementOfWorkVariance item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledStatementOfWorkVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
