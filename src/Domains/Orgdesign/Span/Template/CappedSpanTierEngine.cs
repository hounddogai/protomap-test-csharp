namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped span tier calculation.</summary>
public interface ICappedSpanTierFactor
{
    string Describe();
    double Adjust(CappedSpanTier item, double valueValue);
}

public sealed record TieredCappedSpanTierAdjuster(List<double> Bounds, List<double> Rates) : ICappedSpanTierFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedSpanTier item, double valueValue)
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

public sealed record MinimumCappedSpanTierAdjuster(double Minimum) : ICappedSpanTierFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedSpanTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelCappedSpanTierAdjuster(Dictionary<string, double> Factors) : ICappedSpanTierFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CappedSpanTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentCappedSpanTierAdjuster(double Percent) : ICappedSpanTierFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedSpanTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped span tier steps in order and keeps a trace of each.</summary>
public sealed class CappedSpanTierEngine(List<ICappedSpanTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedSpanTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedSpanTierEngine DefaultCappedSpanTierEngine()
    {
        return new CappedSpanTierEngine(
            new List<ICappedSpanTierFactor>
            {
                new TieredCappedSpanTierAdjuster(
                    new List<double> { 4880.0, 8303.0 },
                    new List<double> { 1.18, 0.94 }),
                new MinimumCappedSpanTierAdjuster(3.9),
                new LabelCappedSpanTierAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.1, ["online"] = 1.19, ["west"] = 1.03 }),
                new PercentCappedSpanTierAdjuster(2.7),
            },
            new List<string>(),
            3.4,
            83134.6);
    }

    public double Evaluate(CappedSpanTier item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedSpanTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
