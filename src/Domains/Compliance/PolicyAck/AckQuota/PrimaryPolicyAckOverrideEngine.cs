namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

/// <summary>One step of the primary policy ack override calculation.</summary>
public interface IPrimaryPolicyAckOverrideStep
{
    string Describe();
    double Adjust(PrimaryPolicyAckOverride item, double valueValue);
}

public sealed record CapPrimaryPolicyAckOverrideAdjuster(double Cap) : IPrimaryPolicyAckOverrideStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryPolicyAckOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitlePrimaryPolicyAckOverrideAdjuster(Dictionary<string, double> Factors) : IPrimaryPolicyAckOverrideStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PrimaryPolicyAckOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PrimaryPolicyAckOverrideHoursOffsetAdjuster(double Weight) : IPrimaryPolicyAckOverrideStep
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(PrimaryPolicyAckOverride item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

/// <summary>Runs the primary policy ack override steps in order and keeps a trace of each.</summary>
public sealed class PrimaryPolicyAckOverrideEngine(List<IPrimaryPolicyAckOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryPolicyAckOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryPolicyAckOverrideEngine DefaultPrimaryPolicyAckOverrideEngine()
    {
        return new PrimaryPolicyAckOverrideEngine(
            new List<IPrimaryPolicyAckOverrideStep>
            {
                new CapPrimaryPolicyAckOverrideAdjuster(10396.0),
                new TitlePrimaryPolicyAckOverrideAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.19, ["retail"] = 1.26, ["north"] = 1.4 }),
                new PrimaryPolicyAckOverrideHoursOffsetAdjuster(1.7),
            },
            new List<string>(),
            2.0,
            95427.4);
    }

    public double Evaluate(PrimaryPolicyAckOverride item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryPolicyAckOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
