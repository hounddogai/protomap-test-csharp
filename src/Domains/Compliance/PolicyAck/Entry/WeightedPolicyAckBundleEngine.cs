namespace Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>One step of the weighted policy ack bundle calculation.</summary>
public interface IWeightedPolicyAckBundleStep
{
    string Describe();
    double Adjust(WeightedPolicyAckBundle item, double valueValue);
}

public sealed record WeightedPolicyAckBundleBatchSizeOffsetAdjuster(double Weight) : IWeightedPolicyAckBundleStep
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(WeightedPolicyAckBundle item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record PercentWeightedPolicyAckBundleAdjuster(double Percent) : IWeightedPolicyAckBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedPolicyAckBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWeightedPolicyAckBundleAdjuster(double Cap) : IWeightedPolicyAckBundleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedPolicyAckBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelWeightedPolicyAckBundleAdjuster(Dictionary<string, double> Factors) : IWeightedPolicyAckBundleStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(WeightedPolicyAckBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredWeightedPolicyAckBundleAdjuster(List<double> Bounds, List<double> Rates) : IWeightedPolicyAckBundleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedPolicyAckBundle item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record MinimumWeightedPolicyAckBundleAdjuster(double Minimum) : IWeightedPolicyAckBundleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedPolicyAckBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the weighted policy ack bundle steps in order and keeps a trace of each.</summary>
public sealed class WeightedPolicyAckBundleEngine(List<IWeightedPolicyAckBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedPolicyAckBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedPolicyAckBundleEngine DefaultWeightedPolicyAckBundleEngine()
    {
        return new WeightedPolicyAckBundleEngine(
            new List<IWeightedPolicyAckBundleStep>
            {
                new WeightedPolicyAckBundleBatchSizeOffsetAdjuster(1.57),
                new PercentWeightedPolicyAckBundleAdjuster(-7.0),
                new CapWeightedPolicyAckBundleAdjuster(38383.0),
                new LabelWeightedPolicyAckBundleAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.02, ["retail"] = 0.87 }),
                new TieredWeightedPolicyAckBundleAdjuster(
                    new List<double> { 1899.0, 2323.0, 5321.0, 9300.0 },
                    new List<double> { 0.9, 1.0, 1.0, 1.14 }),
                new MinimumWeightedPolicyAckBundleAdjuster(35.7),
            },
            new List<string>(),
            3.7,
            32296.2);
    }

    public double Price(WeightedPolicyAckBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedPolicyAckBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
