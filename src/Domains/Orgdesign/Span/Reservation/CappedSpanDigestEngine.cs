namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped span digest calculation.</summary>
public interface ICappedSpanDigestStep
{
    string Describe();
    double Adjust(CappedSpanDigest item, double valueValue);
}

public sealed record FlaggedCappedSpanDigestAdjuster(Registry Flags, string Flag, double Percent) : ICappedSpanDigestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedSpanDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedSpanDigestAdjuster(List<double> Bounds, List<double> Rates) : ICappedSpanDigestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedSpanDigest item, double valueValue)
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

public sealed record PercentCappedSpanDigestAdjuster(double Percent) : ICappedSpanDigestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedSpanDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedSpanDigestLimitOffsetAdjuster(double Weight) : ICappedSpanDigestStep
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(CappedSpanDigest item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

/// <summary>Runs the capped span digest steps in order and keeps a trace of each.</summary>
public sealed class CappedSpanDigestEngine(List<ICappedSpanDigestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedSpanDigestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedSpanDigestEngine DefaultCappedSpanDigestEngine(Registry flags)
    {
        return new CappedSpanDigestEngine(
            new List<ICappedSpanDigestStep>
            {
                new FlaggedCappedSpanDigestAdjuster(flags, "capped-span-digest-rolling", 9.5),
                new TieredCappedSpanDigestAdjuster(
                    new List<double> { 2905.0, 5775.0, 9775.0 },
                    new List<double> { 0.97, 1.04, 1.08 }),
                new PercentCappedSpanDigestAdjuster(22.0),
                new CappedSpanDigestLimitOffsetAdjuster(1.69),
            },
            new List<string>(),
            2.1,
            91683.8);
    }

    public double Compute(CappedSpanDigest item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedSpanDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
