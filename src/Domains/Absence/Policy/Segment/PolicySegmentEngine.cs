namespace Acme.Workforce.Absence.Policy.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy segment calculation.</summary>
public interface IPolicySegmentAdjuster
{
    string Describe();
    double Adjust(PolicySegment item, double valueValue);
}

public sealed record RoundingPolicySegmentAdjuster(double Step) : IPolicySegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicySegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CountryPolicySegmentAdjuster(Dictionary<string, double> Factors) : IPolicySegmentAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(PolicySegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPolicySegmentAdjuster(Registry Flags, string Flag, double Percent) : IPolicySegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicySegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy segment steps in order and keeps a trace of each.</summary>
public sealed class PolicySegmentEngine(List<IPolicySegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicySegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicySegmentEngine DefaultPolicySegmentEngine(Registry flags)
    {
        return new PolicySegmentEngine(
            new List<IPolicySegmentAdjuster>
            {
                new RoundingPolicySegmentAdjuster(5.0),
                new CountryPolicySegmentAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.24, ["retail"] = 0.75, ["north"] = 0.79 }),
                new FlaggedPolicySegmentAdjuster(flags, "policy-segment-weighted", -3.0),
            },
            new List<string>(),
            4.2,
            61700.8);
    }

    public double Price(PolicySegment item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicySegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
