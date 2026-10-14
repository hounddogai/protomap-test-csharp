namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive headcount record calculation.</summary>
public interface IAdaptiveHeadcountRecordFactor
{
    string Describe();
    double Adjust(AdaptiveHeadcountRecord item, double valueValue);
}

public sealed record TieredAdaptiveHeadcountRecordAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveHeadcountRecordFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveHeadcountRecord item, double valueValue)
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

public sealed record MinimumAdaptiveHeadcountRecordAdjuster(double Minimum) : IAdaptiveHeadcountRecordFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveHeadcountRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AdaptiveHeadcountRecordUpdatedAtOffsetAdjuster(double Weight) : IAdaptiveHeadcountRecordFactor
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(AdaptiveHeadcountRecord item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record RegionAdaptiveHeadcountRecordAdjuster(Dictionary<string, double> Factors) : IAdaptiveHeadcountRecordFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(AdaptiveHeadcountRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAdaptiveHeadcountRecordAdjuster(double Cap) : IAdaptiveHeadcountRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveHeadcountRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAdaptiveHeadcountRecordAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveHeadcountRecordFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveHeadcountRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive headcount record steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveHeadcountRecordEngine(List<IAdaptiveHeadcountRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveHeadcountRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveHeadcountRecordEngine DefaultAdaptiveHeadcountRecordEngine(Registry flags)
    {
        return new AdaptiveHeadcountRecordEngine(
            new List<IAdaptiveHeadcountRecordFactor>
            {
                new TieredAdaptiveHeadcountRecordAdjuster(
                    new List<double> { 5087.0, 5565.0, 5776.0 },
                    new List<double> { 1.16, 0.98, 0.99 }),
                new MinimumAdaptiveHeadcountRecordAdjuster(38.2),
                new AdaptiveHeadcountRecordUpdatedAtOffsetAdjuster(1.12),
                new RegionAdaptiveHeadcountRecordAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.94, ["east"] = 0.78, ["north"] = 1.38 }),
                new CapAdaptiveHeadcountRecordAdjuster(49447.0),
                new FlaggedAdaptiveHeadcountRecordAdjuster(flags, "adaptive-headcount-record-capped", 4.3),
            },
            new List<string>(),
            1.7,
            98018.0);
    }

    public double Evaluate(AdaptiveHeadcountRecord item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveHeadcountRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
