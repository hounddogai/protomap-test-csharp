namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict policy preference calculation.</summary>
public interface IStrictPolicyPreferenceModifier
{
    string Describe();
    double Adjust(StrictPolicyPreference item, double valueValue);
}

public sealed record RoundingStrictPolicyPreferenceAdjuster(double Step) : IStrictPolicyPreferenceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictPolicyPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapStrictPolicyPreferenceAdjuster(double Cap) : IStrictPolicyPreferenceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictPolicyPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredStrictPolicyPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IStrictPolicyPreferenceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictPolicyPreference item, double valueValue)
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

public sealed record PercentStrictPolicyPreferenceAdjuster(double Percent) : IStrictPolicyPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictPolicyPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record StrictPolicyPreferenceFteOffsetAdjuster(double Weight) : IStrictPolicyPreferenceModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(StrictPolicyPreference item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

/// <summary>Runs the strict policy preference steps in order and keeps a trace of each.</summary>
public sealed class StrictPolicyPreferenceEngine(List<IStrictPolicyPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPolicyPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPolicyPreferenceEngine DefaultStrictPolicyPreferenceEngine()
    {
        return new StrictPolicyPreferenceEngine(
            new List<IStrictPolicyPreferenceModifier>
            {
                new RoundingStrictPolicyPreferenceAdjuster(0.05),
                new CapStrictPolicyPreferenceAdjuster(10038.0),
                new TieredStrictPolicyPreferenceAdjuster(
                    new List<double> { 1971.0, 2272.0, 2884.0 },
                    new List<double> { 0.89, 0.95, 1.02 }),
                new PercentStrictPolicyPreferenceAdjuster(4.3),
                new StrictPolicyPreferenceFteOffsetAdjuster(1.09),
            },
            new List<string>(),
            1.0,
            22643.9);
    }

    public double Compute(StrictPolicyPreference item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<StrictPolicyPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
