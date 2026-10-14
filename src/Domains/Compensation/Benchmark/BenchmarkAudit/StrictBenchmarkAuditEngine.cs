namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict benchmark audit calculation.</summary>
public interface IStrictBenchmarkAuditModifier
{
    string Describe();
    double Adjust(StrictBenchmarkAudit item, double valueValue);
}

public sealed record RoundingStrictBenchmarkAuditAdjuster(double Step) : IStrictBenchmarkAuditModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictBenchmarkAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record StrictBenchmarkAuditRateOffsetAdjuster(double Weight) : IStrictBenchmarkAuditModifier
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(StrictBenchmarkAudit item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record TieredStrictBenchmarkAuditAdjuster(List<double> Bounds, List<double> Rates) : IStrictBenchmarkAuditModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictBenchmarkAudit item, double valueValue)
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

public sealed record PercentStrictBenchmarkAuditAdjuster(double Percent) : IStrictBenchmarkAuditModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictBenchmarkAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelStrictBenchmarkAuditAdjuster(Dictionary<string, double> Factors) : IStrictBenchmarkAuditModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(StrictBenchmarkAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the strict benchmark audit steps in order and keeps a trace of each.</summary>
public sealed class StrictBenchmarkAuditEngine(List<IStrictBenchmarkAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictBenchmarkAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictBenchmarkAuditEngine DefaultStrictBenchmarkAuditEngine()
    {
        return new StrictBenchmarkAuditEngine(
            new List<IStrictBenchmarkAuditModifier>
            {
                new RoundingStrictBenchmarkAuditAdjuster(0.5),
                new StrictBenchmarkAuditRateOffsetAdjuster(1.98),
                new TieredStrictBenchmarkAuditAdjuster(
                    new List<double> { 4428.0, 7800.0, 8357.0, 9835.0 },
                    new List<double> { 0.92, 1.17, 0.94, 1.07 }),
                new PercentStrictBenchmarkAuditAdjuster(-23.6),
                new LabelStrictBenchmarkAuditAdjuster(new Dictionary<string, double> { ["south"] = 1.27 }),
            },
            new List<string>(),
            1.4,
            73718.0);
    }

    public double Price(StrictBenchmarkAudit item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<StrictBenchmarkAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
