namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack metric calculation.</summary>
public interface IPolicyAckMetricStep
{
    string Describe();
    double Adjust(PolicyAckMetric item, double valueValue);
}

public sealed record PolicyAckMetricCapacityOffsetAdjuster(double Weight) : IPolicyAckMetricStep
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(PolicyAckMetric item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record FlaggedPolicyAckMetricAdjuster(Registry Flags, string Flag, double Percent) : IPolicyAckMetricStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyAckMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPolicyAckMetricAdjuster(double Percent) : IPolicyAckMetricStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPolicyAckMetricAdjuster(double Minimum) : IPolicyAckMetricStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPolicyAckMetricAdjuster(double Step) : IPolicyAckMetricStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyAckMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPolicyAckMetricAdjuster(double Cap) : IPolicyAckMetricStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the policy ack metric steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckMetricEngine(List<IPolicyAckMetricStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckMetricStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckMetricEngine DefaultPolicyAckMetricEngine(Registry flags)
    {
        return new PolicyAckMetricEngine(
            new List<IPolicyAckMetricStep>
            {
                new PolicyAckMetricCapacityOffsetAdjuster(0.38),
                new FlaggedPolicyAckMetricAdjuster(flags, "policy-ack-metric-scheduled", -3.1),
                new PercentPolicyAckMetricAdjuster(22.4),
                new MinimumPolicyAckMetricAdjuster(13.4),
                new RoundingPolicyAckMetricAdjuster(0.5),
                new CapPolicyAckMetricAdjuster(39121.0),
            },
            new List<string>(),
            3.6,
            95508.3);
    }

    public double Run(PolicyAckMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
