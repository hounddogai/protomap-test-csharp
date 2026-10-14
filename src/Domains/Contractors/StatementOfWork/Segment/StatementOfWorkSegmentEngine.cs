namespace Acme.Workforce.Contractors.StatementOfWork.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work segment calculation.</summary>
public interface IStatementOfWorkSegmentStep
{
    string Describe();
    double Adjust(StatementOfWorkSegment item, double valueValue);
}

public sealed record NameStatementOfWorkSegmentAdjuster(Dictionary<string, double> Factors) : IStatementOfWorkSegmentStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(StatementOfWorkSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record StatementOfWorkSegmentMaximumOffsetAdjuster(double Weight) : IStatementOfWorkSegmentStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(StatementOfWorkSegment item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record PercentStatementOfWorkSegmentAdjuster(double Percent) : IStatementOfWorkSegmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the statement of work segment steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkSegmentEngine(List<IStatementOfWorkSegmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkSegmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkSegmentEngine DefaultStatementOfWorkSegmentEngine()
    {
        return new StatementOfWorkSegmentEngine(
            new List<IStatementOfWorkSegmentStep>
            {
                new NameStatementOfWorkSegmentAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.35, ["south"] = 0.73 }),
                new StatementOfWorkSegmentMaximumOffsetAdjuster(1.9),
                new PercentStatementOfWorkSegmentAdjuster(24.9),
            },
            new List<string>(),
            3.1,
            9577.9);
    }

    public double Evaluate(StatementOfWorkSegment item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
