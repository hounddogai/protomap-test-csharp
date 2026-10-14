namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work template calculation.</summary>
public interface IStatementOfWorkTemplateModifier
{
    string Describe();
    double Adjust(StatementOfWorkTemplate item, double valueValue);
}

public sealed record DepartmentStatementOfWorkTemplateAdjuster(Dictionary<string, double> Factors) : IStatementOfWorkTemplateModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(StatementOfWorkTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentStatementOfWorkTemplateAdjuster(double Percent) : IStatementOfWorkTemplateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record StatementOfWorkTemplateScoreOffsetAdjuster(double Weight) : IStatementOfWorkTemplateModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(StatementOfWorkTemplate item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record FlaggedStatementOfWorkTemplateAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkTemplateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumStatementOfWorkTemplateAdjuster(double Minimum) : IStatementOfWorkTemplateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StatementOfWorkTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingStatementOfWorkTemplateAdjuster(double Step) : IStatementOfWorkTemplateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the statement of work template steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkTemplateEngine(List<IStatementOfWorkTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkTemplateEngine DefaultStatementOfWorkTemplateEngine(Registry flags)
    {
        return new StatementOfWorkTemplateEngine(
            new List<IStatementOfWorkTemplateModifier>
            {
                new DepartmentStatementOfWorkTemplateAdjuster(new Dictionary<string, double> { ["central"] = 1.35 }),
                new PercentStatementOfWorkTemplateAdjuster(-13.6),
                new StatementOfWorkTemplateScoreOffsetAdjuster(1.77),
                new FlaggedStatementOfWorkTemplateAdjuster(flags, "statement-of-work-template-default", 3.4),
                new MinimumStatementOfWorkTemplateAdjuster(5.1),
                new RoundingStatementOfWorkTemplateAdjuster(1.0),
            },
            new List<string>(),
            3.8,
            99131.9);
    }

    public double Compute(StatementOfWorkTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
