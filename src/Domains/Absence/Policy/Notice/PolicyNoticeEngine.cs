namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy notice calculation.</summary>
public interface IPolicyNoticeStep
{
    string Describe();
    double Adjust(PolicyNotice item, double valueValue);
}

public sealed record MinimumPolicyNoticeAdjuster(double Minimum) : IPolicyNoticeStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPolicyNoticeAdjuster(Registry Flags, string Flag, double Percent) : IPolicyNoticeStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentPolicyNoticeAdjuster(Dictionary<string, double> Factors) : IPolicyNoticeStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(PolicyNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPolicyNoticeAdjuster(double Step) : IPolicyNoticeStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the policy notice steps in order and keeps a trace of each.</summary>
public sealed class PolicyNoticeEngine(List<IPolicyNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyNoticeEngine DefaultPolicyNoticeEngine(Registry flags)
    {
        return new PolicyNoticeEngine(
            new List<IPolicyNoticeStep>
            {
                new MinimumPolicyNoticeAdjuster(3.5),
                new FlaggedPolicyNoticeAdjuster(flags, "policy-notice-tiered", 7.3),
                new SegmentPolicyNoticeAdjuster(new Dictionary<string, double> { ["south"] = 1.17 }),
                new RoundingPolicyNoticeAdjuster(5.0),
            },
            new List<string>(),
            3.5,
            53391.5);
    }

    public double Compute(PolicyNotice item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
