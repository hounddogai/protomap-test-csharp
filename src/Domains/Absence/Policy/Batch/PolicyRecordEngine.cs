namespace Acme.Workforce.Absence.Policy.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy record calculation.</summary>
public interface IPolicyRecordAdjuster
{
    string Describe();
    double Adjust(PolicyRecord item, double valueValue);
}

public sealed record LabelPolicyRecordAdjuster(Dictionary<string, double> Factors) : IPolicyRecordAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PolicyRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PolicyRecordWeightOffsetAdjuster(double Weight) : IPolicyRecordAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(PolicyRecord item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record FlaggedPolicyRecordAdjuster(Registry Flags, string Flag, double Percent) : IPolicyRecordAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy record steps in order and keeps a trace of each.</summary>
public sealed class PolicyRecordEngine(List<IPolicyRecordAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyRecordAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyRecordEngine DefaultPolicyRecordEngine(Registry flags)
    {
        return new PolicyRecordEngine(
            new List<IPolicyRecordAdjuster>
            {
                new LabelPolicyRecordAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.34, ["retail"] = 1.27 }),
                new PolicyRecordWeightOffsetAdjuster(1.35),
                new FlaggedPolicyRecordAdjuster(flags, "policy-record-regional", 5.4),
            },
            new List<string>(),
            3.7,
            28281.0);
    }

    public double Run(PolicyRecord item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
