namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work record calculation.</summary>
public interface IStatementOfWorkRecordStep
{
    string Describe();
    double Adjust(StatementOfWorkRecord item, double valueValue);
}

public sealed record CapStatementOfWorkRecordAdjuster(double Cap) : IStatementOfWorkRecordStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingStatementOfWorkRecordAdjuster(double Step) : IStatementOfWorkRecordStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredStatementOfWorkRecordAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkRecordStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkRecord item, double valueValue)
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

public sealed record PercentStatementOfWorkRecordAdjuster(double Percent) : IStatementOfWorkRecordStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedStatementOfWorkRecordAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkRecordStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryStatementOfWorkRecordAdjuster(Dictionary<string, double> Factors) : IStatementOfWorkRecordStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(StatementOfWorkRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the statement of work record steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkRecordEngine(List<IStatementOfWorkRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkRecordEngine DefaultStatementOfWorkRecordEngine(Registry flags)
    {
        return new StatementOfWorkRecordEngine(
            new List<IStatementOfWorkRecordStep>
            {
                new CapStatementOfWorkRecordAdjuster(34110.0),
                new RoundingStatementOfWorkRecordAdjuster(0.25),
                new TieredStatementOfWorkRecordAdjuster(
                    new List<double> { 570.0, 3181.0, 3898.0, 7207.0 },
                    new List<double> { 1.04, 1.07, 1.11, 0.81 }),
                new PercentStatementOfWorkRecordAdjuster(10.8),
                new FlaggedStatementOfWorkRecordAdjuster(flags, "statement-of-work-record-regional", -9.5),
                new CategoryStatementOfWorkRecordAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.4, ["west"] = 1.05, ["central"] = 1.27 }),
            },
            new List<string>(),
            2.8,
            26907.0);
    }

    public double Evaluate(StatementOfWorkRecord item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
