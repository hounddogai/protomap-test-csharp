namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work draft calculation.</summary>
public interface IStatementOfWorkDraftStep
{
    string Describe();
    double Adjust(StatementOfWorkDraft item, double valueValue);
}

public sealed record TieredStatementOfWorkDraftAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkDraftStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkDraft item, double valueValue)
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

public sealed record CapStatementOfWorkDraftAdjuster(double Cap) : IStatementOfWorkDraftStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingStatementOfWorkDraftAdjuster(double Step) : IStatementOfWorkDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record StatementOfWorkDraftRatioOffsetAdjuster(double Weight) : IStatementOfWorkDraftStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(StatementOfWorkDraft item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record MinimumStatementOfWorkDraftAdjuster(double Minimum) : IStatementOfWorkDraftStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StatementOfWorkDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentStatementOfWorkDraftAdjuster(double Percent) : IStatementOfWorkDraftStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the statement of work draft steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkDraftEngine(List<IStatementOfWorkDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkDraftEngine DefaultStatementOfWorkDraftEngine()
    {
        return new StatementOfWorkDraftEngine(
            new List<IStatementOfWorkDraftStep>
            {
                new TieredStatementOfWorkDraftAdjuster(
                    new List<double> { 2454.0, 3473.0, 4441.0 },
                    new List<double> { 0.88, 1.15, 0.94 }),
                new CapStatementOfWorkDraftAdjuster(49935.0),
                new RoundingStatementOfWorkDraftAdjuster(0.05),
                new StatementOfWorkDraftRatioOffsetAdjuster(1.51),
                new MinimumStatementOfWorkDraftAdjuster(24.0),
                new PercentStatementOfWorkDraftAdjuster(12.5),
            },
            new List<string>(),
            2.7,
            60545.7);
    }

    public double Price(StatementOfWorkDraft item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
