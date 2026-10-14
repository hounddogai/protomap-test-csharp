namespace Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending benchmark rule calculation.</summary>
public interface IPendingBenchmarkRuleModifier
{
    string Describe();
    double Adjust(PendingBenchmarkRule item, double valueValue);
}

public sealed record CapPendingBenchmarkRuleAdjuster(double Cap) : IPendingBenchmarkRuleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingBenchmarkRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PendingBenchmarkRuleMarginOffsetAdjuster(double Weight) : IPendingBenchmarkRuleModifier
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(PendingBenchmarkRule item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record PercentPendingBenchmarkRuleAdjuster(double Percent) : IPendingBenchmarkRuleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingBenchmarkRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPendingBenchmarkRuleAdjuster(Registry Flags, string Flag, double Percent) : IPendingBenchmarkRuleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingBenchmarkRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelPendingBenchmarkRuleAdjuster(Dictionary<string, double> Factors) : IPendingBenchmarkRuleModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(PendingBenchmarkRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pending benchmark rule steps in order and keeps a trace of each.</summary>
public sealed class PendingBenchmarkRuleEngine(List<IPendingBenchmarkRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingBenchmarkRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingBenchmarkRuleEngine DefaultPendingBenchmarkRuleEngine(Registry flags)
    {
        return new PendingBenchmarkRuleEngine(
            new List<IPendingBenchmarkRuleModifier>
            {
                new CapPendingBenchmarkRuleAdjuster(36834.0),
                new PendingBenchmarkRuleMarginOffsetAdjuster(0.59),
                new PercentPendingBenchmarkRuleAdjuster(14.4),
                new FlaggedPendingBenchmarkRuleAdjuster(flags, "pending-benchmark-rule-adaptive", 5.0),
                new ChannelPendingBenchmarkRuleAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.98, ["north"] = 0.84 }),
            },
            new List<string>(),
            0.7,
            22441.8);
    }

    public double Run(PendingBenchmarkRule item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PendingBenchmarkRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
