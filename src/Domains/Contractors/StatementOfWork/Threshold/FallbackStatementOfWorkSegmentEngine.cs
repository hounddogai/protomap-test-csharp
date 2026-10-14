namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback statement of work segment calculation.</summary>
public interface IFallbackStatementOfWorkSegmentStep
{
    string Describe();
    double Adjust(FallbackStatementOfWorkSegment item, double valueValue);
}

public sealed record FallbackStatementOfWorkSegmentCostOffsetAdjuster(double Weight) : IFallbackStatementOfWorkSegmentStep
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(FallbackStatementOfWorkSegment item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record NameFallbackStatementOfWorkSegmentAdjuster(Dictionary<string, double> Factors) : IFallbackStatementOfWorkSegmentStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(FallbackStatementOfWorkSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapFallbackStatementOfWorkSegmentAdjuster(double Cap) : IFallbackStatementOfWorkSegmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackStatementOfWorkSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingFallbackStatementOfWorkSegmentAdjuster(double Step) : IFallbackStatementOfWorkSegmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackStatementOfWorkSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFallbackStatementOfWorkSegmentAdjuster(double Minimum) : IFallbackStatementOfWorkSegmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackStatementOfWorkSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the fallback statement of work segment steps in order and keeps a trace of each.</summary>
public sealed class FallbackStatementOfWorkSegmentEngine(List<IFallbackStatementOfWorkSegmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackStatementOfWorkSegmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackStatementOfWorkSegmentEngine DefaultFallbackStatementOfWorkSegmentEngine()
    {
        return new FallbackStatementOfWorkSegmentEngine(
            new List<IFallbackStatementOfWorkSegmentStep>
            {
                new FallbackStatementOfWorkSegmentCostOffsetAdjuster(1.36),
                new NameFallbackStatementOfWorkSegmentAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.06, ["east"] = 0.77, ["retail"] = 1.34 }),
                new CapFallbackStatementOfWorkSegmentAdjuster(9238.0),
                new RoundingFallbackStatementOfWorkSegmentAdjuster(0.1),
                new MinimumFallbackStatementOfWorkSegmentAdjuster(29.0),
            },
            new List<string>(),
            3.7,
            83315.7);
    }

    public double Compute(FallbackStatementOfWorkSegment item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackStatementOfWorkSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
