namespace Acme.Workforce.Compensation.Benchmark.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark audit calculation.</summary>
public interface IBenchmarkAuditStep
{
    string Describe();
    double Adjust(BenchmarkAudit item, double valueValue);
}

public sealed record BenchmarkAuditGraceDaysOffsetAdjuster(double Weight) : IBenchmarkAuditStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(BenchmarkAudit item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record FlaggedBenchmarkAuditAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkAuditStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentBenchmarkAuditAdjuster(double Percent) : IBenchmarkAuditStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBenchmarkAuditAdjuster(double Step) : IBenchmarkAuditStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BenchmarkAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the benchmark audit steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkAuditEngine(List<IBenchmarkAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkAuditEngine DefaultBenchmarkAuditEngine(Registry flags)
    {
        return new BenchmarkAuditEngine(
            new List<IBenchmarkAuditStep>
            {
                new BenchmarkAuditGraceDaysOffsetAdjuster(0.14),
                new FlaggedBenchmarkAuditAdjuster(flags, "benchmark-audit-strict", 8.2),
                new PercentBenchmarkAuditAdjuster(10.3),
                new RoundingBenchmarkAuditAdjuster(1.0),
            },
            new List<string>(),
            3.5,
            15523.1);
    }

    public double Price(BenchmarkAudit item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
