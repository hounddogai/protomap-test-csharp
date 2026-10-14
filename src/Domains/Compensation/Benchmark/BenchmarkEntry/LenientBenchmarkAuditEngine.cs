namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient benchmark audit calculation.</summary>
public interface ILenientBenchmarkAuditAdjuster
{
    string Describe();
    double Adjust(LenientBenchmarkAudit item, double valueValue);
}

public sealed record FlaggedLenientBenchmarkAuditAdjuster(Registry Flags, string Flag, double Percent) : ILenientBenchmarkAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientBenchmarkAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientBenchmarkAuditAdjuster(List<double> Bounds, List<double> Rates) : ILenientBenchmarkAuditAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientBenchmarkAudit item, double valueValue)
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

public sealed record MinimumLenientBenchmarkAuditAdjuster(double Minimum) : ILenientBenchmarkAuditAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientBenchmarkAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLenientBenchmarkAuditAdjuster(double Percent) : ILenientBenchmarkAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientBenchmarkAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleLenientBenchmarkAuditAdjuster(Dictionary<string, double> Factors) : ILenientBenchmarkAuditAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(LenientBenchmarkAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient benchmark audit steps in order and keeps a trace of each.</summary>
public sealed class LenientBenchmarkAuditEngine(List<ILenientBenchmarkAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientBenchmarkAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientBenchmarkAuditEngine DefaultLenientBenchmarkAuditEngine(Registry flags)
    {
        return new LenientBenchmarkAuditEngine(
            new List<ILenientBenchmarkAuditAdjuster>
            {
                new FlaggedLenientBenchmarkAuditAdjuster(flags, "lenient-benchmark-audit-fallback", 7.5),
                new TieredLenientBenchmarkAuditAdjuster(
                    new List<double> { 8886.0, 9114.0 },
                    new List<double> { 0.81, 0.95 }),
                new MinimumLenientBenchmarkAuditAdjuster(13.2),
                new PercentLenientBenchmarkAuditAdjuster(-5.5),
                new LocaleLenientBenchmarkAuditAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.34, ["west"] = 1.0, ["online"] = 1.15 }),
            },
            new List<string>(),
            4.6,
            70924.2);
    }

    public double Price(LenientBenchmarkAudit item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LenientBenchmarkAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
