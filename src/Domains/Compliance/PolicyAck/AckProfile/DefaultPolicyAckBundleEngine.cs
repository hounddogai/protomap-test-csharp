namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default policy ack bundle calculation.</summary>
public interface IDefaultPolicyAckBundleFactor
{
    string Describe();
    double Adjust(DefaultPolicyAckBundle item, double valueValue);
}

public sealed record RoundingDefaultPolicyAckBundleAdjuster(double Step) : IDefaultPolicyAckBundleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultPolicyAckBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapDefaultPolicyAckBundleAdjuster(double Cap) : IDefaultPolicyAckBundleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultPolicyAckBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumDefaultPolicyAckBundleAdjuster(double Minimum) : IDefaultPolicyAckBundleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPolicyAckBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDefaultPolicyAckBundleAdjuster(double Percent) : IDefaultPolicyAckBundleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPolicyAckBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default policy ack bundle steps in order and keeps a trace of each.</summary>
public sealed class DefaultPolicyAckBundleEngine(List<IDefaultPolicyAckBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPolicyAckBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPolicyAckBundleEngine DefaultDefaultPolicyAckBundleEngine()
    {
        return new DefaultPolicyAckBundleEngine(
            new List<IDefaultPolicyAckBundleFactor>
            {
                new RoundingDefaultPolicyAckBundleAdjuster(1.0),
                new CapDefaultPolicyAckBundleAdjuster(45975.0),
                new MinimumDefaultPolicyAckBundleAdjuster(13.3),
                new PercentDefaultPolicyAckBundleAdjuster(23.3),
            },
            new List<string>(),
            1.7,
            17717.6);
    }

    public double Compute(DefaultPolicyAckBundle item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPolicyAckBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
