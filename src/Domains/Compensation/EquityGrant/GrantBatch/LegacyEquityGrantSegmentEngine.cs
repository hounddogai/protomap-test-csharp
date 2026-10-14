namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy equity grant segment calculation.</summary>
public interface ILegacyEquityGrantSegmentModifier
{
    string Describe();
    double Adjust(LegacyEquityGrantSegment item, double valueValue);
}

public sealed record RoundingLegacyEquityGrantSegmentAdjuster(double Step) : ILegacyEquityGrantSegmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyEquityGrantSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLegacyEquityGrantSegmentAdjuster(List<double> Bounds, List<double> Rates) : ILegacyEquityGrantSegmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyEquityGrantSegment item, double valueValue)
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

public sealed record TitleLegacyEquityGrantSegmentAdjuster(Dictionary<string, double> Factors) : ILegacyEquityGrantSegmentModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LegacyEquityGrantSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLegacyEquityGrantSegmentAdjuster(double Minimum) : ILegacyEquityGrantSegmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyEquityGrantSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy equity grant segment steps in order and keeps a trace of each.</summary>
public sealed class LegacyEquityGrantSegmentEngine(List<ILegacyEquityGrantSegmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyEquityGrantSegmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyEquityGrantSegmentEngine DefaultLegacyEquityGrantSegmentEngine()
    {
        return new LegacyEquityGrantSegmentEngine(
            new List<ILegacyEquityGrantSegmentModifier>
            {
                new RoundingLegacyEquityGrantSegmentAdjuster(5.0),
                new TieredLegacyEquityGrantSegmentAdjuster(
                    new List<double> { 941.0, 4361.0 },
                    new List<double> { 0.84, 1.15 }),
                new TitleLegacyEquityGrantSegmentAdjuster(new Dictionary<string, double> { ["east"] = 1.23 }),
                new MinimumLegacyEquityGrantSegmentAdjuster(42.5),
            },
            new List<string>(),
            1.1,
            62535.6);
    }

    public double Compute(LegacyEquityGrantSegment item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyEquityGrantSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
