namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered span draft calculation.</summary>
public interface ITieredSpanDraftFactor
{
    string Describe();
    double Adjust(TieredSpanDraft item, double valueValue);
}

public sealed record FlaggedTieredSpanDraftAdjuster(Registry Flags, string Flag, double Percent) : ITieredSpanDraftFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredSpanDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSpanDraftDiscountOffsetAdjuster(double Weight) : ITieredSpanDraftFactor
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(TieredSpanDraft item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record LabelTieredSpanDraftAdjuster(Dictionary<string, double> Factors) : ITieredSpanDraftFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TieredSpanDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentTieredSpanDraftAdjuster(double Percent) : ITieredSpanDraftFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredSpanDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredSpanDraftAdjuster(double Step) : ITieredSpanDraftFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredSpanDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tiered span draft steps in order and keeps a trace of each.</summary>
public sealed class TieredSpanDraftEngine(List<ITieredSpanDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredSpanDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredSpanDraftEngine DefaultTieredSpanDraftEngine(Registry flags)
    {
        return new TieredSpanDraftEngine(
            new List<ITieredSpanDraftFactor>
            {
                new FlaggedTieredSpanDraftAdjuster(flags, "tiered-span-draft-batched", 1.0),
                new TieredSpanDraftDiscountOffsetAdjuster(1.6),
                new LabelTieredSpanDraftAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.24, ["retail"] = 1.37, ["north"] = 0.72 }),
                new PercentTieredSpanDraftAdjuster(-15.0),
                new RoundingTieredSpanDraftAdjuster(0.1),
            },
            new List<string>(),
            4.8,
            36626.7);
    }

    public double Compute(TieredSpanDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<TieredSpanDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
