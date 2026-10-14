namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched retro pay policy calculation.</summary>
public interface IBatchedRetroPayPolicyFactor
{
    string Describe();
    double Adjust(BatchedRetroPayPolicy item, double valueValue);
}

public sealed record MinimumBatchedRetroPayPolicyAdjuster(double Minimum) : IBatchedRetroPayPolicyFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedRetroPayPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingBatchedRetroPayPolicyAdjuster(double Step) : IBatchedRetroPayPolicyFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedRetroPayPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocaleBatchedRetroPayPolicyAdjuster(Dictionary<string, double> Factors) : IBatchedRetroPayPolicyFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(BatchedRetroPayPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBatchedRetroPayPolicyAdjuster(Registry Flags, string Flag, double Percent) : IBatchedRetroPayPolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedRetroPayPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBatchedRetroPayPolicyAdjuster(double Cap) : IBatchedRetroPayPolicyFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedRetroPayPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentBatchedRetroPayPolicyAdjuster(double Percent) : IBatchedRetroPayPolicyFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedRetroPayPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched retro pay policy steps in order and keeps a trace of each.</summary>
public sealed class BatchedRetroPayPolicyEngine(List<IBatchedRetroPayPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedRetroPayPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedRetroPayPolicyEngine DefaultBatchedRetroPayPolicyEngine(Registry flags)
    {
        return new BatchedRetroPayPolicyEngine(
            new List<IBatchedRetroPayPolicyFactor>
            {
                new MinimumBatchedRetroPayPolicyAdjuster(22.7),
                new RoundingBatchedRetroPayPolicyAdjuster(0.25),
                new LocaleBatchedRetroPayPolicyAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.28, ["retail"] = 0.79, ["west"] = 0.8 }),
                new FlaggedBatchedRetroPayPolicyAdjuster(flags, "batched-retro-pay-policy-weighted", 5.9),
                new CapBatchedRetroPayPolicyAdjuster(19197.0),
                new PercentBatchedRetroPayPolicyAdjuster(0.3),
            },
            new List<string>(),
            2.6,
            1274.0);
    }

    public double Compute(BatchedRetroPayPolicy item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedRetroPayPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
