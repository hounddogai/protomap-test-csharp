namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg preference calculation.</summary>
public interface IReorgPreferenceModifier
{
    string Describe();
    double Adjust(ReorgPreference item, double valueValue);
}

public sealed record RoundingReorgPreferenceAdjuster(double Step) : IReorgPreferenceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredReorgPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IReorgPreferenceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgPreference item, double valueValue)
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

public sealed record MinimumReorgPreferenceAdjuster(double Minimum) : IReorgPreferenceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the reorg preference steps in order and keeps a trace of each.</summary>
public sealed class ReorgPreferenceEngine(List<IReorgPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgPreferenceEngine DefaultReorgPreferenceEngine()
    {
        return new ReorgPreferenceEngine(
            new List<IReorgPreferenceModifier>
            {
                new RoundingReorgPreferenceAdjuster(5.0),
                new TieredReorgPreferenceAdjuster(
                    new List<double> { 1446.0, 3417.0 },
                    new List<double> { 0.81, 0.8 }),
                new MinimumReorgPreferenceAdjuster(19.0),
            },
            new List<string>(),
            0.1,
            39842.3);
    }

    public double Price(ReorgPreference item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
