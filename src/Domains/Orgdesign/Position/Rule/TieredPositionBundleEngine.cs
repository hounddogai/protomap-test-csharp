namespace Acme.Workforce.Orgdesign.Position.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered position bundle calculation.</summary>
public interface ITieredPositionBundleFactor
{
    string Describe();
    double Adjust(TieredPositionBundle item, double valueValue);
}

public sealed record TieredTieredPositionBundleAdjuster(List<double> Bounds, List<double> Rates) : ITieredPositionBundleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredPositionBundle item, double valueValue)
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

public sealed record MinimumTieredPositionBundleAdjuster(double Minimum) : ITieredPositionBundleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredPositionBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SegmentTieredPositionBundleAdjuster(Dictionary<string, double> Factors) : ITieredPositionBundleFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(TieredPositionBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPositionBundleMinimumOffsetAdjuster(double Weight) : ITieredPositionBundleFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(TieredPositionBundle item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

/// <summary>Runs the tiered position bundle steps in order and keeps a trace of each.</summary>
public sealed class TieredPositionBundleEngine(List<ITieredPositionBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredPositionBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredPositionBundleEngine DefaultTieredPositionBundleEngine()
    {
        return new TieredPositionBundleEngine(
            new List<ITieredPositionBundleFactor>
            {
                new TieredTieredPositionBundleAdjuster(
                    new List<double> { 3437.0, 4041.0 },
                    new List<double> { 0.98, 1.19 }),
                new MinimumTieredPositionBundleAdjuster(48.2),
                new SegmentTieredPositionBundleAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.31, ["central"] = 1.18, ["retail"] = 0.71 }),
                new TieredPositionBundleMinimumOffsetAdjuster(0.8),
            },
            new List<string>(),
            3.8,
            32210.6);
    }

    public double Run(TieredPositionBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<TieredPositionBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
