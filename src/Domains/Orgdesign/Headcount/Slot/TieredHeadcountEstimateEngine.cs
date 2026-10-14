namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered headcount estimate calculation.</summary>
public interface ITieredHeadcountEstimateStep
{
    string Describe();
    double Adjust(TieredHeadcountEstimate item, double valueValue);
}

public sealed record PercentTieredHeadcountEstimateAdjuster(double Percent) : ITieredHeadcountEstimateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredHeadcountEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedTieredHeadcountEstimateAdjuster(Registry Flags, string Flag, double Percent) : ITieredHeadcountEstimateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredHeadcountEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredHeadcountEstimateAdjuster(double Step) : ITieredHeadcountEstimateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredHeadcountEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredHeadcountEstimateVersionOffsetAdjuster(double Weight) : ITieredHeadcountEstimateStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(TieredHeadcountEstimate item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record MinimumTieredHeadcountEstimateAdjuster(double Minimum) : ITieredHeadcountEstimateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredHeadcountEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tiered headcount estimate steps in order and keeps a trace of each.</summary>
public sealed class TieredHeadcountEstimateEngine(List<ITieredHeadcountEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredHeadcountEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredHeadcountEstimateEngine DefaultTieredHeadcountEstimateEngine(Registry flags)
    {
        return new TieredHeadcountEstimateEngine(
            new List<ITieredHeadcountEstimateStep>
            {
                new PercentTieredHeadcountEstimateAdjuster(-11.8),
                new FlaggedTieredHeadcountEstimateAdjuster(flags, "tiered-headcount-estimate-manual", -5.8),
                new RoundingTieredHeadcountEstimateAdjuster(1.0),
                new TieredHeadcountEstimateVersionOffsetAdjuster(0.51),
                new MinimumTieredHeadcountEstimateAdjuster(41.8),
            },
            new List<string>(),
            0.1,
            73148.7);
    }

    public double Price(TieredHeadcountEstimate item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredHeadcountEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
