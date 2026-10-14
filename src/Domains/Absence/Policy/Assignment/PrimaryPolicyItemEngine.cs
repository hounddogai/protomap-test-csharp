namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary policy item calculation.</summary>
public interface IPrimaryPolicyItemAdjuster
{
    string Describe();
    double Adjust(PrimaryPolicyItem item, double valueValue);
}

public sealed record PrimaryPolicyItemScoreOffsetAdjuster(double Weight) : IPrimaryPolicyItemAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(PrimaryPolicyItem item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record RoundingPrimaryPolicyItemAdjuster(double Step) : IPrimaryPolicyItemAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryPolicyItem item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPrimaryPolicyItemAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryPolicyItemAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryPolicyItem item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPrimaryPolicyItemAdjuster(double Percent) : IPrimaryPolicyItemAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryPolicyItem item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary policy item steps in order and keeps a trace of each.</summary>
public sealed class PrimaryPolicyItemEngine(List<IPrimaryPolicyItemAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryPolicyItemAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryPolicyItemEngine DefaultPrimaryPolicyItemEngine(Registry flags)
    {
        return new PrimaryPolicyItemEngine(
            new List<IPrimaryPolicyItemAdjuster>
            {
                new PrimaryPolicyItemScoreOffsetAdjuster(0.25),
                new RoundingPrimaryPolicyItemAdjuster(1.0),
                new FlaggedPrimaryPolicyItemAdjuster(flags, "primary-policy-item-strict", -5.4),
                new PercentPrimaryPolicyItemAdjuster(11.5),
            },
            new List<string>(),
            2.5,
            82440.3);
    }

    public double Price(PrimaryPolicyItem item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryPolicyItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
