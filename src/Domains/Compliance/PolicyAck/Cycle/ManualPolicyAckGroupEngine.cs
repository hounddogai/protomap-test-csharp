namespace Acme.Workforce.Compliance.PolicyAck.Cycle;

/// <summary>One step of the manual policy ack group calculation.</summary>
public interface IManualPolicyAckGroupStep
{
    string Describe();
    double Adjust(ManualPolicyAckGroup item, double valueValue);
}

public sealed record TieredManualPolicyAckGroupAdjuster(List<double> Bounds, List<double> Rates) : IManualPolicyAckGroupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualPolicyAckGroup item, double valueValue)
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

public sealed record CapManualPolicyAckGroupAdjuster(double Cap) : IManualPolicyAckGroupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualPolicyAckGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameManualPolicyAckGroupAdjuster(Dictionary<string, double> Factors) : IManualPolicyAckGroupStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ManualPolicyAckGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumManualPolicyAckGroupAdjuster(double Minimum) : IManualPolicyAckGroupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualPolicyAckGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the manual policy ack group steps in order and keeps a trace of each.</summary>
public sealed class ManualPolicyAckGroupEngine(List<IManualPolicyAckGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualPolicyAckGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualPolicyAckGroupEngine DefaultManualPolicyAckGroupEngine()
    {
        return new ManualPolicyAckGroupEngine(
            new List<IManualPolicyAckGroupStep>
            {
                new TieredManualPolicyAckGroupAdjuster(
                    new List<double> { 981.0, 4625.0, 5703.0, 9279.0 },
                    new List<double> { 1.17, 1.04, 0.8, 0.82 }),
                new CapManualPolicyAckGroupAdjuster(18532.0),
                new NameManualPolicyAckGroupAdjuster(new Dictionary<string, double> { ["retail"] = 0.71 }),
                new MinimumManualPolicyAckGroupAdjuster(18.0),
            },
            new List<string>(),
            1.5,
            48165.5);
    }

    public double Evaluate(ManualPolicyAckGroup item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ManualPolicyAckGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
