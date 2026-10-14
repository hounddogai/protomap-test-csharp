namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending cost model policy calculation.</summary>
public interface IPendingCostModelPolicyStep
{
    string Describe();
    double Adjust(PendingCostModelPolicy item, double valueValue);
}

public sealed record MinimumPendingCostModelPolicyAdjuster(double Minimum) : IPendingCostModelPolicyStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingCostModelPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPendingCostModelPolicyAdjuster(Registry Flags, string Flag, double Percent) : IPendingCostModelPolicyStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingCostModelPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPendingCostModelPolicyAdjuster(double Cap) : IPendingCostModelPolicyStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingCostModelPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ChannelPendingCostModelPolicyAdjuster(Dictionary<string, double> Factors) : IPendingCostModelPolicyStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(PendingCostModelPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pending cost model policy steps in order and keeps a trace of each.</summary>
public sealed class PendingCostModelPolicyEngine(List<IPendingCostModelPolicyStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingCostModelPolicyStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingCostModelPolicyEngine DefaultPendingCostModelPolicyEngine(Registry flags)
    {
        return new PendingCostModelPolicyEngine(
            new List<IPendingCostModelPolicyStep>
            {
                new MinimumPendingCostModelPolicyAdjuster(24.2),
                new FlaggedPendingCostModelPolicyAdjuster(flags, "pending-cost-model-policy-adaptive", -2.8),
                new CapPendingCostModelPolicyAdjuster(8711.0),
                new ChannelPendingCostModelPolicyAdjuster(new Dictionary<string, double> { ["east"] = 1.13 }),
            },
            new List<string>(),
            0.4,
            45265.5);
    }

    public double Compute(PendingCostModelPolicy item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PendingCostModelPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
