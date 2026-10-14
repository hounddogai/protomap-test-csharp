namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work milestone calculation.</summary>
public interface IStatementOfWorkMilestoneFactor
{
    string Describe();
    double Adjust(StatementOfWorkMilestone item, double valueValue);
}

public sealed record TieredStatementOfWorkMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkMilestoneFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkMilestone item, double valueValue)
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

public sealed record RoundingStatementOfWorkMilestoneAdjuster(double Step) : IStatementOfWorkMilestoneFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record StatementOfWorkMilestoneMinimumOffsetAdjuster(double Weight) : IStatementOfWorkMilestoneFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(StatementOfWorkMilestone item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record FlaggedStatementOfWorkMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the statement of work milestone steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkMilestoneEngine(List<IStatementOfWorkMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkMilestoneEngine DefaultStatementOfWorkMilestoneEngine(Registry flags)
    {
        return new StatementOfWorkMilestoneEngine(
            new List<IStatementOfWorkMilestoneFactor>
            {
                new TieredStatementOfWorkMilestoneAdjuster(
                    new List<double> { 4230.0, 8690.0 },
                    new List<double> { 1.15, 1.17 }),
                new RoundingStatementOfWorkMilestoneAdjuster(5.0),
                new StatementOfWorkMilestoneMinimumOffsetAdjuster(1.66),
                new FlaggedStatementOfWorkMilestoneAdjuster(flags, "statement-of-work-milestone-lenient", 6.2),
            },
            new List<string>(),
            3.1,
            51468.4);
    }

    public double Evaluate(StatementOfWorkMilestone item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
