namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span request calculation.</summary>
public interface ISpanRequestModifier
{
    string Describe();
    double Adjust(SpanRequest item, double valueValue);
}

public sealed record CategorySpanRequestAdjuster(Dictionary<string, double> Factors) : ISpanRequestModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(SpanRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSpanRequestAdjuster(double Step) : ISpanRequestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapSpanRequestAdjuster(double Cap) : ISpanRequestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedSpanRequestAdjuster(Registry Flags, string Flag, double Percent) : ISpanRequestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SpanRequestRetriesOffsetAdjuster(double Weight) : ISpanRequestModifier
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(SpanRequest item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

/// <summary>Runs the span request steps in order and keeps a trace of each.</summary>
public sealed class SpanRequestEngine(List<ISpanRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanRequestEngine DefaultSpanRequestEngine(Registry flags)
    {
        return new SpanRequestEngine(
            new List<ISpanRequestModifier>
            {
                new CategorySpanRequestAdjuster(new Dictionary<string, double> { ["south"] = 1.2, ["east"] = 0.72 }),
                new RoundingSpanRequestAdjuster(0.05),
                new CapSpanRequestAdjuster(11404.0),
                new FlaggedSpanRequestAdjuster(flags, "span-request-adaptive", -0.7),
                new SpanRequestRetriesOffsetAdjuster(1.9),
            },
            new List<string>(),
            3.0,
            11518.2);
    }

    public double Price(SpanRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<SpanRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
