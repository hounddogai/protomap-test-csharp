namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work mapping calculation.</summary>
public interface IStatementOfWorkMappingStep
{
    string Describe();
    double Adjust(StatementOfWorkMapping item, double valueValue);
}

public sealed record CapStatementOfWorkMappingAdjuster(double Cap) : IStatementOfWorkMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingStatementOfWorkMappingAdjuster(double Step) : IStatementOfWorkMappingStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentStatementOfWorkMappingAdjuster(double Percent) : IStatementOfWorkMappingStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the statement of work mapping steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkMappingEngine(List<IStatementOfWorkMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkMappingEngine DefaultStatementOfWorkMappingEngine()
    {
        return new StatementOfWorkMappingEngine(
            new List<IStatementOfWorkMappingStep>
            {
                new CapStatementOfWorkMappingAdjuster(27352.0),
                new RoundingStatementOfWorkMappingAdjuster(5.0),
                new PercentStatementOfWorkMappingAdjuster(-23.0),
            },
            new List<string>(),
            2.3,
            67957.3);
    }

    public double Compute(StatementOfWorkMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
