namespace Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>One step of the tiered policy ack variance calculation.</summary>
public interface ITieredPolicyAckVarianceStep
{
    string Describe();
    double Adjust(TieredPolicyAckVariance item, double valueValue);
}

public sealed record TieredTieredPolicyAckVarianceAdjuster(List<double> Bounds, List<double> Rates) : ITieredPolicyAckVarianceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredPolicyAckVariance item, double valueValue)
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

public sealed record MinimumTieredPolicyAckVarianceAdjuster(double Minimum) : ITieredPolicyAckVarianceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredPolicyAckVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTieredPolicyAckVarianceAdjuster(double Cap) : ITieredPolicyAckVarianceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredPolicyAckVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTieredPolicyAckVarianceAdjuster(double Percent) : ITieredPolicyAckVarianceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredPolicyAckVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered policy ack variance steps in order and keeps a trace of each.</summary>
public sealed class TieredPolicyAckVarianceEngine(List<ITieredPolicyAckVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredPolicyAckVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredPolicyAckVarianceEngine DefaultTieredPolicyAckVarianceEngine()
    {
        return new TieredPolicyAckVarianceEngine(
            new List<ITieredPolicyAckVarianceStep>
            {
                new TieredTieredPolicyAckVarianceAdjuster(
                    new List<double> { 2005.0, 3000.0, 3592.0, 4845.0 },
                    new List<double> { 1.08, 1.04, 1.01, 0.86 }),
                new MinimumTieredPolicyAckVarianceAdjuster(3.4),
                new CapTieredPolicyAckVarianceAdjuster(21220.0),
                new PercentTieredPolicyAckVarianceAdjuster(-20.2),
            },
            new List<string>(),
            4.1,
            38147.3);
    }

    public double Run(TieredPolicyAckVariance item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredPolicyAckVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
