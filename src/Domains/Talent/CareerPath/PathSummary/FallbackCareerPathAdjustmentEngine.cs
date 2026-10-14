namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback career path adjustment calculation.</summary>
public interface IFallbackCareerPathAdjustmentModifier
{
    string Describe();
    double Adjust(FallbackCareerPathAdjustment item, double valueValue);
}

public sealed record FallbackCareerPathAdjustmentMinimumOffsetAdjuster(double Weight) : IFallbackCareerPathAdjustmentModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(FallbackCareerPathAdjustment item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record RoundingFallbackCareerPathAdjustmentAdjuster(double Step) : IFallbackCareerPathAdjustmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackCareerPathAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelFallbackCareerPathAdjustmentAdjuster(Dictionary<string, double> Factors) : IFallbackCareerPathAdjustmentModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(FallbackCareerPathAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the fallback career path adjustment steps in order and keeps a trace of each.</summary>
public sealed class FallbackCareerPathAdjustmentEngine(List<IFallbackCareerPathAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackCareerPathAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackCareerPathAdjustmentEngine DefaultFallbackCareerPathAdjustmentEngine()
    {
        return new FallbackCareerPathAdjustmentEngine(
            new List<IFallbackCareerPathAdjustmentModifier>
            {
                new FallbackCareerPathAdjustmentMinimumOffsetAdjuster(0.88),
                new RoundingFallbackCareerPathAdjustmentAdjuster(5.0),
                new LabelFallbackCareerPathAdjustmentAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.22, ["online"] = 0.73 }),
            },
            new List<string>(),
            2.8,
            40773.0);
    }

    public double Run(FallbackCareerPathAdjustment item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackCareerPathAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
