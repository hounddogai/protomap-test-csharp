namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work request calculation.</summary>
public interface IStatementOfWorkRequestModifier
{
    string Describe();
    double Adjust(StatementOfWorkRequest item, double valueValue);
}

public sealed record CapStatementOfWorkRequestAdjuster(double Cap) : IStatementOfWorkRequestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedStatementOfWorkRequestAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkRequestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentStatementOfWorkRequestAdjuster(double Percent) : IStatementOfWorkRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStatementOfWorkRequestAdjuster(double Step) : IStatementOfWorkRequestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredStatementOfWorkRequestAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkRequest item, double valueValue)
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

/// <summary>Runs the statement of work request steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkRequestEngine(List<IStatementOfWorkRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkRequestEngine DefaultStatementOfWorkRequestEngine(Registry flags)
    {
        return new StatementOfWorkRequestEngine(
            new List<IStatementOfWorkRequestModifier>
            {
                new CapStatementOfWorkRequestAdjuster(14259.0),
                new FlaggedStatementOfWorkRequestAdjuster(flags, "statement-of-work-request-adaptive", 5.1),
                new PercentStatementOfWorkRequestAdjuster(-17.0),
                new RoundingStatementOfWorkRequestAdjuster(0.1),
                new TieredStatementOfWorkRequestAdjuster(
                    new List<double> { 3182.0, 3742.0, 7985.0 },
                    new List<double> { 1.09, 0.87, 1.1 }),
            },
            new List<string>(),
            1.7,
            67114.2);
    }

    public double Run(StatementOfWorkRequest item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
