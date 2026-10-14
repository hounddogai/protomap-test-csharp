namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the policy rollup calculation.</summary>
public interface IPolicyRollupStep
{
    string Describe();
    double Adjust(PolicyRollup item, double valueValue);
}

public sealed record FlaggedPolicyRollupAdjuster(Registry Flags, string Flag, double Percent) : IPolicyRollupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitlePolicyRollupAdjuster(Dictionary<string, double> Factors) : IPolicyRollupStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PolicyRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPolicyRollupAdjuster(double Minimum) : IPolicyRollupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PolicyRollupRevisionOffsetAdjuster(double Weight) : IPolicyRollupStep
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(PolicyRollup item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

/// <summary>Runs the policy rollup steps in order and keeps a trace of each.</summary>
public sealed class PolicyRollupEngine(List<IPolicyRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyRollupEngine DefaultPolicyRollupEngine(Registry flags)
    {
        return new PolicyRollupEngine(
            new List<IPolicyRollupStep>
            {
                new FlaggedPolicyRollupAdjuster(flags, "policy-rollup-global", 2.1),
                new TitlePolicyRollupAdjuster(new Dictionary<string, double> { ["west"] = 1.33 }),
                new MinimumPolicyRollupAdjuster(22.5),
                new PolicyRollupRevisionOffsetAdjuster(1.38),
            },
            new List<string>(),
            1.6,
            14036.1);
    }

    public double Evaluate(PolicyRollup item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
