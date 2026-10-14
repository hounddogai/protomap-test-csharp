namespace Acme.Workforce.Engagement.Recognition.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback recognition baseline calculation.</summary>
public interface IFallbackRecognitionBaselineModifier
{
    string Describe();
    double Adjust(FallbackRecognitionBaseline item, double valueValue);
}

public sealed record MinimumFallbackRecognitionBaselineAdjuster(double Minimum) : IFallbackRecognitionBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackRecognitionBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingFallbackRecognitionBaselineAdjuster(double Step) : IFallbackRecognitionBaselineModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackRecognitionBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFallbackRecognitionBaselineAdjuster(double Percent) : IFallbackRecognitionBaselineModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackRecognitionBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitFallbackRecognitionBaselineAdjuster(Dictionary<string, double> Factors) : IFallbackRecognitionBaselineModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(FallbackRecognitionBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredFallbackRecognitionBaselineAdjuster(List<double> Bounds, List<double> Rates) : IFallbackRecognitionBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackRecognitionBaseline item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

/// <summary>Runs the fallback recognition baseline steps in order and keeps a trace of each.</summary>
public sealed class FallbackRecognitionBaselineEngine(List<IFallbackRecognitionBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackRecognitionBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackRecognitionBaselineEngine DefaultFallbackRecognitionBaselineEngine()
    {
        return new FallbackRecognitionBaselineEngine(
            new List<IFallbackRecognitionBaselineModifier>
            {
                new MinimumFallbackRecognitionBaselineAdjuster(41.7),
                new RoundingFallbackRecognitionBaselineAdjuster(0.05),
                new PercentFallbackRecognitionBaselineAdjuster(-0.9),
                new UnitFallbackRecognitionBaselineAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.39, ["retail"] = 0.95 }),
                new TieredFallbackRecognitionBaselineAdjuster(
                    new List<double> { 753.0, 6215.0, 8209.0 },
                    new List<double> { 0.87, 1.07, 1.08 }),
            },
            new List<string>(),
            4.1,
            16089.6);
    }

    public double Price(FallbackRecognitionBaseline item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackRecognitionBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
