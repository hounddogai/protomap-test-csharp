namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global asset return record calculation.</summary>
public interface IGlobalAssetReturnRecordModifier
{
    string Describe();
    double Adjust(GlobalAssetReturnRecord item, double valueValue);
}

public sealed record SegmentGlobalAssetReturnRecordAdjuster(Dictionary<string, double> Factors) : IGlobalAssetReturnRecordModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(GlobalAssetReturnRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredGlobalAssetReturnRecordAdjuster(List<double> Bounds, List<double> Rates) : IGlobalAssetReturnRecordModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalAssetReturnRecord item, double valueValue)
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

public sealed record RoundingGlobalAssetReturnRecordAdjuster(double Step) : IGlobalAssetReturnRecordModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalAssetReturnRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record GlobalAssetReturnRecordCapacityOffsetAdjuster(double Weight) : IGlobalAssetReturnRecordModifier
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(GlobalAssetReturnRecord item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record PercentGlobalAssetReturnRecordAdjuster(double Percent) : IGlobalAssetReturnRecordModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalAssetReturnRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global asset return record steps in order and keeps a trace of each.</summary>
public sealed class GlobalAssetReturnRecordEngine(List<IGlobalAssetReturnRecordModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalAssetReturnRecordModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalAssetReturnRecordEngine DefaultGlobalAssetReturnRecordEngine()
    {
        return new GlobalAssetReturnRecordEngine(
            new List<IGlobalAssetReturnRecordModifier>
            {
                new SegmentGlobalAssetReturnRecordAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.83, ["online"] = 0.91 }),
                new TieredGlobalAssetReturnRecordAdjuster(
                    new List<double> { 994.0, 1935.0, 6502.0, 8600.0 },
                    new List<double> { 0.9, 1.02, 0.84, 0.94 }),
                new RoundingGlobalAssetReturnRecordAdjuster(1.0),
                new GlobalAssetReturnRecordCapacityOffsetAdjuster(1.3),
                new PercentGlobalAssetReturnRecordAdjuster(24.5),
            },
            new List<string>(),
            2.2,
            45141.4);
    }

    public double Evaluate(GlobalAssetReturnRecord item)
    {
        var current = item.NetExposure();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<GlobalAssetReturnRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
