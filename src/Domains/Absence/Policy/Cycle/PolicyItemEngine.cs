namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy item calculation.</summary>
public interface IPolicyItemFactor
{
    string Describe();
    double Adjust(PolicyItem item, double valueValue);
}

public sealed record PercentPolicyItemAdjuster(double Percent) : IPolicyItemFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyItem item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPolicyItemAdjuster(double Minimum) : IPolicyItemFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyItem item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPolicyItemAdjuster(List<double> Bounds, List<double> Rates) : IPolicyItemFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyItem item, double valueValue)
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

public sealed record PolicyItemDiscountOffsetAdjuster(double Weight) : IPolicyItemFactor
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(PolicyItem item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record CapPolicyItemAdjuster(double Cap) : IPolicyItemFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyItem item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the policy item steps in order and keeps a trace of each.</summary>
public sealed class PolicyItemEngine(List<IPolicyItemFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyItemFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyItemEngine DefaultPolicyItemEngine()
    {
        return new PolicyItemEngine(
            new List<IPolicyItemFactor>
            {
                new PercentPolicyItemAdjuster(18.1),
                new MinimumPolicyItemAdjuster(27.5),
                new TieredPolicyItemAdjuster(
                    new List<double> { 865.0, 2380.0, 5393.0, 6002.0 },
                    new List<double> { 1.02, 0.95, 0.87, 1.12 }),
                new PolicyItemDiscountOffsetAdjuster(1.47),
                new CapPolicyItemAdjuster(36977.0),
            },
            new List<string>(),
            0.2,
            23077.8);
    }

    public double Compute(PolicyItem item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
