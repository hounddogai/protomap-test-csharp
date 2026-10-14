namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark policy calculation.</summary>
public interface IBenchmarkPolicyStep
{
    string Describe();
    double Adjust(BenchmarkPolicy item, double valueValue);
}

public sealed record MinimumBenchmarkPolicyAdjuster(double Minimum) : IBenchmarkPolicyStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BenchmarkPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record BenchmarkPolicyBatchSizeOffsetAdjuster(double Weight) : IBenchmarkPolicyStep
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(BenchmarkPolicy item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record NameBenchmarkPolicyAdjuster(Dictionary<string, double> Factors) : IBenchmarkPolicyStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BenchmarkPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapBenchmarkPolicyAdjuster(double Cap) : IBenchmarkPolicyStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BenchmarkPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBenchmarkPolicyAdjuster(double Step) : IBenchmarkPolicyStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BenchmarkPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBenchmarkPolicyAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkPolicyStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the benchmark policy steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkPolicyEngine(List<IBenchmarkPolicyStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkPolicyStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkPolicyEngine DefaultBenchmarkPolicyEngine(Registry flags)
    {
        return new BenchmarkPolicyEngine(
            new List<IBenchmarkPolicyStep>
            {
                new MinimumBenchmarkPolicyAdjuster(47.3),
                new BenchmarkPolicyBatchSizeOffsetAdjuster(1.68),
                new NameBenchmarkPolicyAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.22, ["north"] = 0.79 }),
                new CapBenchmarkPolicyAdjuster(16334.0),
                new RoundingBenchmarkPolicyAdjuster(0.5),
                new FlaggedBenchmarkPolicyAdjuster(flags, "benchmark-policy-tiered", 1.9),
            },
            new List<string>(),
            1.0,
            80033.1);
    }

    public double Price(BenchmarkPolicy item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
