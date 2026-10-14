namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback recognition override calculation.</summary>
public interface IFallbackRecognitionOverrideFactor
{
    string Describe();
    double Adjust(FallbackRecognitionOverride item, double valueValue);
}

public sealed record PercentFallbackRecognitionOverrideAdjuster(double Percent) : IFallbackRecognitionOverrideFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackRecognitionOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionFallbackRecognitionOverrideAdjuster(Dictionary<string, double> Factors) : IFallbackRecognitionOverrideFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(FallbackRecognitionOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumFallbackRecognitionOverrideAdjuster(double Minimum) : IFallbackRecognitionOverrideFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackRecognitionOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the fallback recognition override steps in order and keeps a trace of each.</summary>
public sealed class FallbackRecognitionOverrideEngine(List<IFallbackRecognitionOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackRecognitionOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackRecognitionOverrideEngine DefaultFallbackRecognitionOverrideEngine()
    {
        return new FallbackRecognitionOverrideEngine(
            new List<IFallbackRecognitionOverrideFactor>
            {
                new PercentFallbackRecognitionOverrideAdjuster(-21.3),
                new RegionFallbackRecognitionOverrideAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.73, ["west"] = 1.11 }),
                new MinimumFallbackRecognitionOverrideAdjuster(45.0),
            },
            new List<string>(),
            3.6,
            47875.6);
    }

    public double Run(FallbackRecognitionOverride item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackRecognitionOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
