namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work exception calculation.</summary>
public interface IStatementOfWorkExceptionModifier
{
    string Describe();
    double Adjust(StatementOfWorkException item, double valueValue);
}

public sealed record PercentStatementOfWorkExceptionAdjuster(double Percent) : IStatementOfWorkExceptionModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStatementOfWorkExceptionAdjuster(double Step) : IStatementOfWorkExceptionModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedStatementOfWorkExceptionAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkExceptionModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the statement of work exception steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkExceptionEngine(List<IStatementOfWorkExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkExceptionEngine DefaultStatementOfWorkExceptionEngine(Registry flags)
    {
        return new StatementOfWorkExceptionEngine(
            new List<IStatementOfWorkExceptionModifier>
            {
                new PercentStatementOfWorkExceptionAdjuster(-8.3),
                new RoundingStatementOfWorkExceptionAdjuster(0.25),
                new FlaggedStatementOfWorkExceptionAdjuster(flags, "statement-of-work-exception-global", 6.1),
            },
            new List<string>(),
            0.5,
            1805.3);
    }

    public double Price(StatementOfWorkException item)
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
