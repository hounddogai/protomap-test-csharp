namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor estimate calculation.</summary>
public interface IVisitorEstimateFactor
{
    string Describe();
    double Adjust(VisitorEstimate item, double valueValue);
}

public sealed record RoundingVisitorEstimateAdjuster(double Step) : IVisitorEstimateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SegmentVisitorEstimateAdjuster(Dictionary<string, double> Factors) : IVisitorEstimateFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(VisitorEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record VisitorEstimateRetriesOffsetAdjuster(double Weight) : IVisitorEstimateFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(VisitorEstimate item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

/// <summary>Runs the visitor estimate steps in order and keeps a trace of each.</summary>
public sealed class VisitorEstimateEngine(List<IVisitorEstimateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorEstimateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorEstimateEngine DefaultVisitorEstimateEngine()
    {
        return new VisitorEstimateEngine(
            new List<IVisitorEstimateFactor>
            {
                new RoundingVisitorEstimateAdjuster(0.05),
                new SegmentVisitorEstimateAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.82, ["retail"] = 1.29, ["south"] = 1.07 }),
                new VisitorEstimateRetriesOffsetAdjuster(0.31),
            },
            new List<string>(),
            3.4,
            70025.2);
    }

    public double Run(VisitorEstimate item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
