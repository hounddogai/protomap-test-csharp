namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual career path entry calculation.</summary>
public interface IManualCareerPathEntryModifier
{
    string Describe();
    double Adjust(ManualCareerPathEntry item, double valueValue);
}

public sealed record MinimumManualCareerPathEntryAdjuster(double Minimum) : IManualCareerPathEntryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualCareerPathEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredManualCareerPathEntryAdjuster(List<double> Bounds, List<double> Rates) : IManualCareerPathEntryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualCareerPathEntry item, double valueValue)
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

public sealed record RoundingManualCareerPathEntryAdjuster(double Step) : IManualCareerPathEntryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualCareerPathEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapManualCareerPathEntryAdjuster(double Cap) : IManualCareerPathEntryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualCareerPathEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelManualCareerPathEntryAdjuster(Dictionary<string, double> Factors) : IManualCareerPathEntryModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ManualCareerPathEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the manual career path entry steps in order and keeps a trace of each.</summary>
public sealed class ManualCareerPathEntryEngine(List<IManualCareerPathEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualCareerPathEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualCareerPathEntryEngine DefaultManualCareerPathEntryEngine()
    {
        return new ManualCareerPathEntryEngine(
            new List<IManualCareerPathEntryModifier>
            {
                new MinimumManualCareerPathEntryAdjuster(23.3),
                new TieredManualCareerPathEntryAdjuster(
                    new List<double> { 4910.0, 6940.0, 8736.0 },
                    new List<double> { 0.91, 0.94, 1.17 }),
                new RoundingManualCareerPathEntryAdjuster(0.05),
                new CapManualCareerPathEntryAdjuster(4769.0),
                new LabelManualCareerPathEntryAdjuster(new Dictionary<string, double> { ["retail"] = 1.16 }),
            },
            new List<string>(),
            5.0,
            99070.2);
    }

    public double Evaluate(ManualCareerPathEntry item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ManualCareerPathEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
