namespace Acme.Workforce.Absence.Policy.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback policy rule calculation.</summary>
public interface IFallbackPolicyRuleAdjuster
{
    string Describe();
    double Adjust(FallbackPolicyRule item, double valueValue);
}

public sealed record FallbackPolicyRuleRateOffsetAdjuster(double Weight) : IFallbackPolicyRuleAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(FallbackPolicyRule item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record TieredFallbackPolicyRuleAdjuster(List<double> Bounds, List<double> Rates) : IFallbackPolicyRuleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackPolicyRule item, double valueValue)
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

public sealed record MinimumFallbackPolicyRuleAdjuster(double Minimum) : IFallbackPolicyRuleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackPolicyRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentFallbackPolicyRuleAdjuster(double Percent) : IFallbackPolicyRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackPolicyRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFallbackPolicyRuleAdjuster(double Cap) : IFallbackPolicyRuleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackPolicyRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the fallback policy rule steps in order and keeps a trace of each.</summary>
public sealed class FallbackPolicyRuleEngine(List<IFallbackPolicyRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPolicyRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPolicyRuleEngine DefaultFallbackPolicyRuleEngine()
    {
        return new FallbackPolicyRuleEngine(
            new List<IFallbackPolicyRuleAdjuster>
            {
                new FallbackPolicyRuleRateOffsetAdjuster(0.14),
                new TieredFallbackPolicyRuleAdjuster(
                    new List<double> { 4184.0, 8276.0 },
                    new List<double> { 0.84, 0.86 }),
                new MinimumFallbackPolicyRuleAdjuster(19.6),
                new PercentFallbackPolicyRuleAdjuster(-23.9),
                new CapFallbackPolicyRuleAdjuster(40668.0),
            },
            new List<string>(),
            0.2,
            54810.0);
    }

    public double Run(FallbackPolicyRule item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPolicyRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
