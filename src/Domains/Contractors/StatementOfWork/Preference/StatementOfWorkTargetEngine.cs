namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>One step of the statement of work target calculation.</summary>
public interface IStatementOfWorkTargetAdjuster
{
    string Describe();
    double Adjust(StatementOfWorkTarget item, double valueValue);
}

public sealed record CapStatementOfWorkTargetAdjuster(double Cap) : IStatementOfWorkTargetAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumStatementOfWorkTargetAdjuster(double Minimum) : IStatementOfWorkTargetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StatementOfWorkTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CategoryStatementOfWorkTargetAdjuster(Dictionary<string, double> Factors) : IStatementOfWorkTargetAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(StatementOfWorkTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the statement of work target steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkTargetEngine(List<IStatementOfWorkTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkTargetEngine DefaultStatementOfWorkTargetEngine()
    {
        return new StatementOfWorkTargetEngine(
            new List<IStatementOfWorkTargetAdjuster>
            {
                new CapStatementOfWorkTargetAdjuster(18380.0),
                new MinimumStatementOfWorkTargetAdjuster(36.3),
                new CategoryStatementOfWorkTargetAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.8, ["west"] = 1.07 }),
            },
            new List<string>(),
            0.6,
            95976.6);
    }

    public double Evaluate(StatementOfWorkTarget item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
