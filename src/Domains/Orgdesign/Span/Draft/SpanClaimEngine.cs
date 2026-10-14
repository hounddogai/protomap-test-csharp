namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span claim calculation.</summary>
public interface ISpanClaimAdjuster
{
    string Describe();
    double Adjust(SpanClaim item, double valueValue);
}

public sealed record SpanClaimWeightOffsetAdjuster(double Weight) : ISpanClaimAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(SpanClaim item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record RoundingSpanClaimAdjuster(double Step) : ISpanClaimAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CountrySpanClaimAdjuster(Dictionary<string, double> Factors) : ISpanClaimAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(SpanClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapSpanClaimAdjuster(double Cap) : ISpanClaimAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedSpanClaimAdjuster(Registry Flags, string Flag, double Percent) : ISpanClaimAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the span claim steps in order and keeps a trace of each.</summary>
public sealed class SpanClaimEngine(List<ISpanClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanClaimEngine DefaultSpanClaimEngine(Registry flags)
    {
        return new SpanClaimEngine(
            new List<ISpanClaimAdjuster>
            {
                new SpanClaimWeightOffsetAdjuster(1.32),
                new RoundingSpanClaimAdjuster(0.5),
                new CountrySpanClaimAdjuster(new Dictionary<string, double> { ["central"] = 0.7 }),
                new CapSpanClaimAdjuster(19055.0),
                new FlaggedSpanClaimAdjuster(flags, "span-claim-manual", 0.1),
            },
            new List<string>(),
            2.2,
            44387.6);
    }

    public double Compute(SpanClaim item)
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

    public Dictionary<string, double> EvaluateAll(List<SpanClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
