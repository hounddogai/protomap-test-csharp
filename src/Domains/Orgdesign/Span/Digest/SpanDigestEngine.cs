namespace Acme.Workforce.Orgdesign.Span.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span digest calculation.</summary>
public interface ISpanDigestStep
{
    string Describe();
    double Adjust(SpanDigest item, double valueValue);
}

public sealed record FlaggedSpanDigestAdjuster(Registry Flags, string Flag, double Percent) : ISpanDigestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSpanDigestAdjuster(double Percent) : ISpanDigestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SpanDigestMarginOffsetAdjuster(double Weight) : ISpanDigestStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(SpanDigest item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record MinimumSpanDigestAdjuster(double Minimum) : ISpanDigestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSpanDigestAdjuster(double Cap) : ISpanDigestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the span digest steps in order and keeps a trace of each.</summary>
public sealed class SpanDigestEngine(List<ISpanDigestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanDigestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanDigestEngine DefaultSpanDigestEngine(Registry flags)
    {
        return new SpanDigestEngine(
            new List<ISpanDigestStep>
            {
                new FlaggedSpanDigestAdjuster(flags, "span-digest-batched", -1.1),
                new PercentSpanDigestAdjuster(-24.6),
                new SpanDigestMarginOffsetAdjuster(1.32),
                new MinimumSpanDigestAdjuster(13.1),
                new CapSpanDigestAdjuster(29268.0),
            },
            new List<string>(),
            4.4,
            77214.8);
    }

    public double Compute(SpanDigest item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SpanDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
