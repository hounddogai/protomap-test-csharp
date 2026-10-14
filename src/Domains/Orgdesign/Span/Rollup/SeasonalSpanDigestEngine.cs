namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal span digest calculation.</summary>
public interface ISeasonalSpanDigestModifier
{
    string Describe();
    double Adjust(SeasonalSpanDigest item, double valueValue);
}

public sealed record SeasonalSpanDigestAmountOffsetAdjuster(double Weight) : ISeasonalSpanDigestModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(SeasonalSpanDigest item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record RoundingSeasonalSpanDigestAdjuster(double Step) : ISeasonalSpanDigestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalSpanDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSeasonalSpanDigestAdjuster(double Minimum) : ISeasonalSpanDigestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalSpanDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedSeasonalSpanDigestAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalSpanDigestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalSpanDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalSpanDigestAdjuster(double Cap) : ISeasonalSpanDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalSpanDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the seasonal span digest steps in order and keeps a trace of each.</summary>
public sealed class SeasonalSpanDigestEngine(List<ISeasonalSpanDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalSpanDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalSpanDigestEngine DefaultSeasonalSpanDigestEngine(Registry flags)
    {
        return new SeasonalSpanDigestEngine(
            new List<ISeasonalSpanDigestModifier>
            {
                new SeasonalSpanDigestAmountOffsetAdjuster(0.05),
                new RoundingSeasonalSpanDigestAdjuster(0.25),
                new MinimumSeasonalSpanDigestAdjuster(7.5),
                new FlaggedSeasonalSpanDigestAdjuster(flags, "seasonal-span-digest-tiered", -5.2),
                new CapSeasonalSpanDigestAdjuster(23547.0),
            },
            new List<string>(),
            0.2,
            39518.8);
    }

    public double Compute(SeasonalSpanDigest item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalSpanDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
