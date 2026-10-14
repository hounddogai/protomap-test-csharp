namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary span estimate calculation.</summary>
public interface IPrimarySpanEstimateFactor
{
    string Describe();
    double Adjust(PrimarySpanEstimate item, double valueValue);
}

public sealed record LabelPrimarySpanEstimateAdjuster(Dictionary<string, double> Factors) : IPrimarySpanEstimateFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PrimarySpanEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPrimarySpanEstimateAdjuster(double Percent) : IPrimarySpanEstimateFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimarySpanEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimarySpanEstimateAdjuster(double Step) : IPrimarySpanEstimateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimarySpanEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPrimarySpanEstimateAdjuster(double Cap) : IPrimarySpanEstimateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimarySpanEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the primary span estimate steps in order and keeps a trace of each.</summary>
public sealed class PrimarySpanEstimateEngine(List<IPrimarySpanEstimateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimarySpanEstimateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimarySpanEstimateEngine DefaultPrimarySpanEstimateEngine()
    {
        return new PrimarySpanEstimateEngine(
            new List<IPrimarySpanEstimateFactor>
            {
                new LabelPrimarySpanEstimateAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.21, ["central"] = 0.87 }),
                new PercentPrimarySpanEstimateAdjuster(0.2),
                new RoundingPrimarySpanEstimateAdjuster(1.0),
                new CapPrimarySpanEstimateAdjuster(35750.0),
            },
            new List<string>(),
            0.8,
            22571.1);
    }

    public double Price(PrimarySpanEstimate item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimarySpanEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
