namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy variance calculation.</summary>
public interface IPolicyVarianceFactor
{
    string Describe();
    double Adjust(PolicyVariance item, double valueValue);
}

public sealed record MinimumPolicyVarianceAdjuster(double Minimum) : IPolicyVarianceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record UnitPolicyVarianceAdjuster(Dictionary<string, double> Factors) : IPolicyVarianceFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(PolicyVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPolicyVarianceAdjuster(double Cap) : IPolicyVarianceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PolicyVarianceQuantityOffsetAdjuster(double Weight) : IPolicyVarianceFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(PolicyVariance item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record RoundingPolicyVarianceAdjuster(double Step) : IPolicyVarianceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPolicyVarianceAdjuster(double Percent) : IPolicyVarianceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy variance steps in order and keeps a trace of each.</summary>
public sealed class PolicyVarianceEngine(List<IPolicyVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyVarianceEngine DefaultPolicyVarianceEngine()
    {
        return new PolicyVarianceEngine(
            new List<IPolicyVarianceFactor>
            {
                new MinimumPolicyVarianceAdjuster(12.0),
                new UnitPolicyVarianceAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.35, ["retail"] = 0.99 }),
                new CapPolicyVarianceAdjuster(45779.0),
                new PolicyVarianceQuantityOffsetAdjuster(0.3),
                new RoundingPolicyVarianceAdjuster(0.05),
                new PercentPolicyVarianceAdjuster(-23.3),
            },
            new List<string>(),
            0.9,
            58064.0);
    }

    public double Compute(PolicyVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
