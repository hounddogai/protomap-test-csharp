namespace Acme.Workforce.Contractors.Rate.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate summary calculation.</summary>
public interface IRateSummaryStep
{
    string Describe();
    double Adjust(RateSummary item, double valueValue);
}

public sealed record PercentRateSummaryAdjuster(double Percent) : IRateSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRateSummaryAdjuster(double Minimum) : IRateSummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RateSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RateSummaryCostOffsetAdjuster(double Weight) : IRateSummaryStep
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(RateSummary item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

/// <summary>Runs the rate summary steps in order and keeps a trace of each.</summary>
public sealed class RateSummaryEngine(List<IRateSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateSummaryEngine DefaultRateSummaryEngine()
    {
        return new RateSummaryEngine(
            new List<IRateSummaryStep>
            {
                new PercentRateSummaryAdjuster(18.1),
                new MinimumRateSummaryAdjuster(34.8),
                new RateSummaryCostOffsetAdjuster(0.92),
            },
            new List<string>(),
            3.8,
            8983.9);
    }

    public double Evaluate(RateSummary item)
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

    public Dictionary<string, double> EvaluateAll(List<RateSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
