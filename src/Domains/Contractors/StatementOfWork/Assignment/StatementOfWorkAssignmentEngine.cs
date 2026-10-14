namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work assignment calculation.</summary>
public interface IStatementOfWorkAssignmentModifier
{
    string Describe();
    double Adjust(StatementOfWorkAssignment item, double valueValue);
}

public sealed record FlaggedStatementOfWorkAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStatementOfWorkAssignmentAdjuster(double Step) : IStatementOfWorkAssignmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumStatementOfWorkAssignmentAdjuster(double Minimum) : IStatementOfWorkAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StatementOfWorkAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapStatementOfWorkAssignmentAdjuster(double Cap) : IStatementOfWorkAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentStatementOfWorkAssignmentAdjuster(double Percent) : IStatementOfWorkAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the statement of work assignment steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkAssignmentEngine(List<IStatementOfWorkAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkAssignmentEngine DefaultStatementOfWorkAssignmentEngine(Registry flags)
    {
        return new StatementOfWorkAssignmentEngine(
            new List<IStatementOfWorkAssignmentModifier>
            {
                new FlaggedStatementOfWorkAssignmentAdjuster(flags, "statement-of-work-assignment-fallback", 8.5),
                new RoundingStatementOfWorkAssignmentAdjuster(1.0),
                new MinimumStatementOfWorkAssignmentAdjuster(2.8),
                new CapStatementOfWorkAssignmentAdjuster(25382.0),
                new PercentStatementOfWorkAssignmentAdjuster(4.5),
            },
            new List<string>(),
            2.2,
            74232.2);
    }

    public double Price(StatementOfWorkAssignment item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
