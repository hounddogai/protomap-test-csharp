namespace Acme.Workforce.Compensation.PayBand.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy pay band rollup calculation.</summary>
public interface ILegacyPayBandRollupModifier
{
    string Describe();
    double Adjust(LegacyPayBandRollup item, double valueValue);
}

public sealed record MinimumLegacyPayBandRollupAdjuster(double Minimum) : ILegacyPayBandRollupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyPayBandRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLegacyPayBandRollupAdjuster(double Step) : ILegacyPayBandRollupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyPayBandRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLegacyPayBandRollupAdjuster(double Percent) : ILegacyPayBandRollupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyPayBandRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy pay band rollup steps in order and keeps a trace of each.</summary>
public sealed class LegacyPayBandRollupEngine(List<ILegacyPayBandRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPayBandRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPayBandRollupEngine DefaultLegacyPayBandRollupEngine()
    {
        return new LegacyPayBandRollupEngine(
            new List<ILegacyPayBandRollupModifier>
            {
                new MinimumLegacyPayBandRollupAdjuster(47.5),
                new RoundingLegacyPayBandRollupAdjuster(0.1),
                new PercentLegacyPayBandRollupAdjuster(1.6),
            },
            new List<string>(),
            2.9,
            52691.6);
    }

    public double Run(LegacyPayBandRollup item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPayBandRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
