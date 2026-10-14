namespace Acme.Workforce.Assets.Refresh.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped refresh slot calculation.</summary>
public interface ICappedRefreshSlotModifier
{
    string Describe();
    double Adjust(CappedRefreshSlot item, double valueValue);
}

public sealed record RoundingCappedRefreshSlotAdjuster(double Step) : ICappedRefreshSlotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedRefreshSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCappedRefreshSlotAdjuster(List<double> Bounds, List<double> Rates) : ICappedRefreshSlotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedRefreshSlot item, double valueValue)
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

public sealed record LabelCappedRefreshSlotAdjuster(Dictionary<string, double> Factors) : ICappedRefreshSlotModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CappedRefreshSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the capped refresh slot steps in order and keeps a trace of each.</summary>
public sealed class CappedRefreshSlotEngine(List<ICappedRefreshSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedRefreshSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedRefreshSlotEngine DefaultCappedRefreshSlotEngine()
    {
        return new CappedRefreshSlotEngine(
            new List<ICappedRefreshSlotModifier>
            {
                new RoundingCappedRefreshSlotAdjuster(0.5),
                new TieredCappedRefreshSlotAdjuster(
                    new List<double> { 996.0, 6215.0, 6418.0, 6578.0 },
                    new List<double> { 0.98, 1.14, 1.11, 0.99 }),
                new LabelCappedRefreshSlotAdjuster(new Dictionary<string, double> { ["central"] = 1.19 }),
            },
            new List<string>(),
            2.0,
            39148.0);
    }

    public double Run(CappedRefreshSlot item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CappedRefreshSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
