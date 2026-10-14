namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive bonus pool policy calculation.</summary>
public interface IAdaptiveBonusPoolPolicyFactor
{
    string Describe();
    double Adjust(AdaptiveBonusPoolPolicy item, double valueValue);
}

public sealed record CapAdaptiveBonusPoolPolicyAdjuster(double Cap) : IAdaptiveBonusPoolPolicyFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveBonusPoolPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAdaptiveBonusPoolPolicyAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveBonusPoolPolicyFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveBonusPoolPolicy item, double valueValue)
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

public sealed record RoundingAdaptiveBonusPoolPolicyAdjuster(double Step) : IAdaptiveBonusPoolPolicyFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveBonusPoolPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive bonus pool policy steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveBonusPoolPolicyEngine(List<IAdaptiveBonusPoolPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveBonusPoolPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveBonusPoolPolicyEngine DefaultAdaptiveBonusPoolPolicyEngine()
    {
        return new AdaptiveBonusPoolPolicyEngine(
            new List<IAdaptiveBonusPoolPolicyFactor>
            {
                new CapAdaptiveBonusPoolPolicyAdjuster(18763.0),
                new TieredAdaptiveBonusPoolPolicyAdjuster(
                    new List<double> { 3300.0, 8233.0, 8480.0, 8491.0 },
                    new List<double> { 1.18, 1.04, 0.82, 1.11 }),
                new RoundingAdaptiveBonusPoolPolicyAdjuster(1.0),
            },
            new List<string>(),
            3.4,
            56029.6);
    }

    public double Compute(AdaptiveBonusPoolPolicy item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveBonusPoolPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
