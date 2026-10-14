namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback span score calculation.</summary>
public interface IFallbackSpanScoreModifier
{
    string Describe();
    double Adjust(FallbackSpanScore item, double valueValue);
}

public sealed record CapFallbackSpanScoreAdjuster(double Cap) : IFallbackSpanScoreModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackSpanScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumFallbackSpanScoreAdjuster(double Minimum) : IFallbackSpanScoreModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackSpanScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentFallbackSpanScoreAdjuster(double Percent) : IFallbackSpanScoreModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackSpanScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedFallbackSpanScoreAdjuster(Registry Flags, string Flag, double Percent) : IFallbackSpanScoreModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackSpanScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFallbackSpanScoreAdjuster(double Step) : IFallbackSpanScoreModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackSpanScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback span score steps in order and keeps a trace of each.</summary>
public sealed class FallbackSpanScoreEngine(List<IFallbackSpanScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackSpanScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackSpanScoreEngine DefaultFallbackSpanScoreEngine(Registry flags)
    {
        return new FallbackSpanScoreEngine(
            new List<IFallbackSpanScoreModifier>
            {
                new CapFallbackSpanScoreAdjuster(35779.0),
                new MinimumFallbackSpanScoreAdjuster(37.4),
                new PercentFallbackSpanScoreAdjuster(8.5),
                new FlaggedFallbackSpanScoreAdjuster(flags, "fallback-span-score-adaptive", 6.8),
                new RoundingFallbackSpanScoreAdjuster(0.5),
            },
            new List<string>(),
            4.7,
            29437.0);
    }

    public double Run(FallbackSpanScore item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackSpanScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
