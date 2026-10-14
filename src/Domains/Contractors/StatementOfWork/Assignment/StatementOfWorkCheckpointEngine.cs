namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work checkpoint calculation.</summary>
public interface IStatementOfWorkCheckpointAdjuster
{
    string Describe();
    double Adjust(StatementOfWorkCheckpoint item, double valueValue);
}

public sealed record CapStatementOfWorkCheckpointAdjuster(double Cap) : IStatementOfWorkCheckpointAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingStatementOfWorkCheckpointAdjuster(double Step) : IStatementOfWorkCheckpointAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentStatementOfWorkCheckpointAdjuster(double Percent) : IStatementOfWorkCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedStatementOfWorkCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the statement of work checkpoint steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkCheckpointEngine(List<IStatementOfWorkCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkCheckpointEngine DefaultStatementOfWorkCheckpointEngine(Registry flags)
    {
        return new StatementOfWorkCheckpointEngine(
            new List<IStatementOfWorkCheckpointAdjuster>
            {
                new CapStatementOfWorkCheckpointAdjuster(26070.0),
                new RoundingStatementOfWorkCheckpointAdjuster(0.5),
                new PercentStatementOfWorkCheckpointAdjuster(9.2),
                new FlaggedStatementOfWorkCheckpointAdjuster(flags, "statement-of-work-checkpoint-global", 0.1),
            },
            new List<string>(),
            3.0,
            16015.8);
    }

    public double Price(StatementOfWorkCheckpoint item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
