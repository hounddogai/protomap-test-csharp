namespace Acme.Workforce.Contractors.StatementOfWork.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work rollup calculation.</summary>
public interface IStatementOfWorkRollupModifier
{
    string Describe();
    double Adjust(StatementOfWorkRollup item, double valueValue);
}

public sealed record CapStatementOfWorkRollupAdjuster(double Cap) : IStatementOfWorkRollupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentStatementOfWorkRollupAdjuster(double Percent) : IStatementOfWorkRollupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedStatementOfWorkRollupAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkRollupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStatementOfWorkRollupAdjuster(double Step) : IStatementOfWorkRollupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionStatementOfWorkRollupAdjuster(Dictionary<string, double> Factors) : IStatementOfWorkRollupModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(StatementOfWorkRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record StatementOfWorkRollupLimitOffsetAdjuster(double Weight) : IStatementOfWorkRollupModifier
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(StatementOfWorkRollup item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

/// <summary>Runs the statement of work rollup steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkRollupEngine(List<IStatementOfWorkRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkRollupEngine DefaultStatementOfWorkRollupEngine(Registry flags)
    {
        return new StatementOfWorkRollupEngine(
            new List<IStatementOfWorkRollupModifier>
            {
                new CapStatementOfWorkRollupAdjuster(11060.0),
                new PercentStatementOfWorkRollupAdjuster(5.1),
                new FlaggedStatementOfWorkRollupAdjuster(flags, "statement-of-work-rollup-default", 4.1),
                new RoundingStatementOfWorkRollupAdjuster(0.25),
                new RegionStatementOfWorkRollupAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.29, ["north"] = 0.79, ["south"] = 0.97 }),
                new StatementOfWorkRollupLimitOffsetAdjuster(1.75),
            },
            new List<string>(),
            2.4,
            22636.1);
    }

    public double Compute(StatementOfWorkRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
