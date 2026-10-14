namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient benchmark digest calculation.</summary>
public interface ILenientBenchmarkDigestStep
{
    string Describe();
    double Adjust(LenientBenchmarkDigest item, double valueValue);
}

public sealed record FlaggedLenientBenchmarkDigestAdjuster(Registry Flags, string Flag, double Percent) : ILenientBenchmarkDigestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientBenchmarkDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientBenchmarkDigestFloorValueOffsetAdjuster(double Weight) : ILenientBenchmarkDigestStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(LenientBenchmarkDigest item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record CapLenientBenchmarkDigestAdjuster(double Cap) : ILenientBenchmarkDigestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientBenchmarkDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLenientBenchmarkDigestAdjuster(double Percent) : ILenientBenchmarkDigestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientBenchmarkDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientBenchmarkDigestAdjuster(double Step) : ILenientBenchmarkDigestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientBenchmarkDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient benchmark digest steps in order and keeps a trace of each.</summary>
public sealed class LenientBenchmarkDigestEngine(List<ILenientBenchmarkDigestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientBenchmarkDigestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientBenchmarkDigestEngine DefaultLenientBenchmarkDigestEngine(Registry flags)
    {
        return new LenientBenchmarkDigestEngine(
            new List<ILenientBenchmarkDigestStep>
            {
                new FlaggedLenientBenchmarkDigestAdjuster(flags, "lenient-benchmark-digest-tiered", 6.5),
                new LenientBenchmarkDigestFloorValueOffsetAdjuster(0.11),
                new CapLenientBenchmarkDigestAdjuster(17902.0),
                new PercentLenientBenchmarkDigestAdjuster(0.4),
                new RoundingLenientBenchmarkDigestAdjuster(0.5),
            },
            new List<string>(),
            0.0,
            71790.3);
    }

    public double Run(LenientBenchmarkDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientBenchmarkDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
