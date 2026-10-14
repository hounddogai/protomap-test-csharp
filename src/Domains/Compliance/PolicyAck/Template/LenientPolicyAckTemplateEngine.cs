namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient policy ack template calculation.</summary>
public interface ILenientPolicyAckTemplateAdjuster
{
    string Describe();
    double Adjust(LenientPolicyAckTemplate item, double valueValue);
}

public sealed record CapLenientPolicyAckTemplateAdjuster(double Cap) : ILenientPolicyAckTemplateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientPolicyAckTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLenientPolicyAckTemplateAdjuster(double Minimum) : ILenientPolicyAckTemplateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientPolicyAckTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLenientPolicyAckTemplateAdjuster(Registry Flags, string Flag, double Percent) : ILenientPolicyAckTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientPolicyAckTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientPolicyAckTemplateAdjuster(double Step) : ILenientPolicyAckTemplateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientPolicyAckTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLenientPolicyAckTemplateAdjuster(double Percent) : ILenientPolicyAckTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientPolicyAckTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient policy ack template steps in order and keeps a trace of each.</summary>
public sealed class LenientPolicyAckTemplateEngine(List<ILenientPolicyAckTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientPolicyAckTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientPolicyAckTemplateEngine DefaultLenientPolicyAckTemplateEngine(Registry flags)
    {
        return new LenientPolicyAckTemplateEngine(
            new List<ILenientPolicyAckTemplateAdjuster>
            {
                new CapLenientPolicyAckTemplateAdjuster(41018.0),
                new MinimumLenientPolicyAckTemplateAdjuster(26.4),
                new FlaggedLenientPolicyAckTemplateAdjuster(flags, "lenient-policy-ack-template-weighted", 8.6),
                new RoundingLenientPolicyAckTemplateAdjuster(5.0),
                new PercentLenientPolicyAckTemplateAdjuster(-23.7),
            },
            new List<string>(),
            3.7,
            24695.5);
    }

    public double Compute(LenientPolicyAckTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientPolicyAckTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
