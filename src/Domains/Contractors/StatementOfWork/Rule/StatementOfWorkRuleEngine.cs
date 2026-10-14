namespace Acme.Workforce.Contractors.StatementOfWork.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work rule calculation.</summary>
public interface IStatementOfWorkRuleFactor
{
    string Describe();
    double Adjust(StatementOfWorkRule item, double valueValue);
}

public sealed record RegionStatementOfWorkRuleAdjuster(Dictionary<string, double> Factors) : IStatementOfWorkRuleFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(StatementOfWorkRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedStatementOfWorkRuleAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkRuleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record StatementOfWorkRuleEndsAtOffsetAdjuster(double Weight) : IStatementOfWorkRuleFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(StatementOfWorkRule item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record RoundingStatementOfWorkRuleAdjuster(double Step) : IStatementOfWorkRuleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapStatementOfWorkRuleAdjuster(double Cap) : IStatementOfWorkRuleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentStatementOfWorkRuleAdjuster(double Percent) : IStatementOfWorkRuleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the statement of work rule steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkRuleEngine(List<IStatementOfWorkRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkRuleEngine DefaultStatementOfWorkRuleEngine(Registry flags)
    {
        return new StatementOfWorkRuleEngine(
            new List<IStatementOfWorkRuleFactor>
            {
                new RegionStatementOfWorkRuleAdjuster(new Dictionary<string, double> { ["north"] = 1.34 }),
                new FlaggedStatementOfWorkRuleAdjuster(flags, "statement-of-work-rule-fallback", 7.2),
                new StatementOfWorkRuleEndsAtOffsetAdjuster(0.82),
                new RoundingStatementOfWorkRuleAdjuster(0.1),
                new CapStatementOfWorkRuleAdjuster(675.0),
                new PercentStatementOfWorkRuleAdjuster(-20.2),
            },
            new List<string>(),
            1.6,
            19897.3);
    }

    public double Run(StatementOfWorkRule item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
