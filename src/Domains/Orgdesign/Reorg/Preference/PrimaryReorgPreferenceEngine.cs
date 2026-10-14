namespace Acme.Workforce.Orgdesign.Reorg.Preference;

/// <summary>One step of the primary reorg preference calculation.</summary>
public interface IPrimaryReorgPreferenceModifier
{
    string Describe();
    double Adjust(PrimaryReorgPreference item, double valueValue);
}

public sealed record RegionPrimaryReorgPreferenceAdjuster(Dictionary<string, double> Factors) : IPrimaryReorgPreferenceModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(PrimaryReorgPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPrimaryReorgPreferenceAdjuster(double Cap) : IPrimaryReorgPreferenceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryReorgPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPrimaryReorgPreferenceAdjuster(double Percent) : IPrimaryReorgPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryReorgPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary reorg preference steps in order and keeps a trace of each.</summary>
public sealed class PrimaryReorgPreferenceEngine(List<IPrimaryReorgPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryReorgPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryReorgPreferenceEngine DefaultPrimaryReorgPreferenceEngine()
    {
        return new PrimaryReorgPreferenceEngine(
            new List<IPrimaryReorgPreferenceModifier>
            {
                new RegionPrimaryReorgPreferenceAdjuster(new Dictionary<string, double> { ["south"] = 1.25 }),
                new CapPrimaryReorgPreferenceAdjuster(19174.0),
                new PercentPrimaryReorgPreferenceAdjuster(-16.6),
            },
            new List<string>(),
            0.4,
            16769.5);
    }

    public double Evaluate(PrimaryReorgPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryReorgPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
