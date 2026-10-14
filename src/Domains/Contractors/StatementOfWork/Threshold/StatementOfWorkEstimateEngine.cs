namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work estimate calculation.</summary>
public interface IStatementOfWorkEstimateStep
{
    string Describe();
    double Adjust(StatementOfWorkEstimate item, double valueValue);
}

public sealed record FlaggedStatementOfWorkEstimateAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkEstimateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStatementOfWorkEstimateAdjuster(double Step) : IStatementOfWorkEstimateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record StatementOfWorkEstimateStartsAtOffsetAdjuster(double Weight) : IStatementOfWorkEstimateStep
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(StatementOfWorkEstimate item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record LabelStatementOfWorkEstimateAdjuster(Dictionary<string, double> Factors) : IStatementOfWorkEstimateStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(StatementOfWorkEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the statement of work estimate steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkEstimateEngine(List<IStatementOfWorkEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkEstimateEngine DefaultStatementOfWorkEstimateEngine(Registry flags)
    {
        return new StatementOfWorkEstimateEngine(
            new List<IStatementOfWorkEstimateStep>
            {
                new FlaggedStatementOfWorkEstimateAdjuster(flags, "statement-of-work-estimate-capped", 9.4),
                new RoundingStatementOfWorkEstimateAdjuster(0.05),
                new StatementOfWorkEstimateStartsAtOffsetAdjuster(1.15),
                new LabelStatementOfWorkEstimateAdjuster(new Dictionary<string, double> { ["north"] = 1.37 }),
            },
            new List<string>(),
            3.0,
            54050.0);
    }

    public double Price(StatementOfWorkEstimate item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
