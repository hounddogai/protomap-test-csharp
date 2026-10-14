namespace Acme.Workforce.Compliance.Exception.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default exception digest calculation.</summary>
public interface IDefaultExceptionDigestFactor
{
    string Describe();
    double Adjust(DefaultExceptionDigest item, double valueValue);
}

public sealed record RoundingDefaultExceptionDigestAdjuster(double Step) : IDefaultExceptionDigestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultExceptionDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDefaultExceptionDigestAdjuster(Registry Flags, string Flag, double Percent) : IDefaultExceptionDigestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultExceptionDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultExceptionDigestAdjuster(List<double> Bounds, List<double> Rates) : IDefaultExceptionDigestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultExceptionDigest item, double valueValue)
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

public sealed record DefaultExceptionDigestQuantityOffsetAdjuster(double Weight) : IDefaultExceptionDigestFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(DefaultExceptionDigest item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record SegmentDefaultExceptionDigestAdjuster(Dictionary<string, double> Factors) : IDefaultExceptionDigestFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(DefaultExceptionDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumDefaultExceptionDigestAdjuster(double Minimum) : IDefaultExceptionDigestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultExceptionDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default exception digest steps in order and keeps a trace of each.</summary>
public sealed class DefaultExceptionDigestEngineExtra(List<IDefaultExceptionDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultExceptionDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public double Run(DefaultExceptionDigest item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultExceptionDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}

/// <summary>Functions of default exception digest engine.</summary>
public static class DefaultExceptionDigestEngine
{
    public static DefaultExceptionDigestEngineExtra DefaultDefaultExceptionDigestEngine(Registry flags)
    {
        return new DefaultExceptionDigestEngineExtra(
            new List<IDefaultExceptionDigestFactor>
            {
                new RoundingDefaultExceptionDigestAdjuster(1.0),
                new FlaggedDefaultExceptionDigestAdjuster(flags, "default-exception-digest-tiered", -7.3),
                new TieredDefaultExceptionDigestAdjuster(
                    new List<double> { 568.0, 6337.0, 8171.0 },
                    new List<double> { 1.02, 0.88, 1.08 }),
                new DefaultExceptionDigestQuantityOffsetAdjuster(0.6),
                new SegmentDefaultExceptionDigestAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.37, ["east"] = 1.05 }),
                new MinimumDefaultExceptionDigestAdjuster(6.3),
            },
            new List<string>(),
            1.7,
            88560.2);
    }
}
