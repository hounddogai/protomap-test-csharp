namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the headcount policy calculation.</summary>
public interface IHeadcountPolicyAdjuster
{
    string Describe();
    double Adjust(HeadcountPolicy item, double valueValue);
}

public sealed record FlaggedHeadcountPolicyAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumHeadcountPolicyAdjuster(double Minimum) : IHeadcountPolicyAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ChannelHeadcountPolicyAdjuster(Dictionary<string, double> Factors) : IHeadcountPolicyAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(HeadcountPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record HeadcountPolicyCostOffsetAdjuster(double Weight) : IHeadcountPolicyAdjuster
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(HeadcountPolicy item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

/// <summary>Runs the headcount policy steps in order and keeps a trace of each.</summary>
public sealed class HeadcountPolicyEngine(List<IHeadcountPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountPolicyEngine DefaultHeadcountPolicyEngine(Registry flags)
    {
        return new HeadcountPolicyEngine(
            new List<IHeadcountPolicyAdjuster>
            {
                new FlaggedHeadcountPolicyAdjuster(flags, "headcount-policy-legacy", 5.8),
                new MinimumHeadcountPolicyAdjuster(4.3),
                new ChannelHeadcountPolicyAdjuster(new Dictionary<string, double> { ["west"] = 0.75 }),
                new HeadcountPolicyCostOffsetAdjuster(1.75),
            },
            new List<string>(),
            4.9,
            53857.0);
    }

    public double Compute(HeadcountPolicy item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
