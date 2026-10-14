namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

/// <summary>One step of the adaptive skill profile snapshot calculation.</summary>
public interface IAdaptiveSkillProfileSnapshotStep
{
    string Describe();
    double Adjust(AdaptiveSkillProfileSnapshot item, double valueValue);
}

public sealed record TieredAdaptiveSkillProfileSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveSkillProfileSnapshotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveSkillProfileSnapshot item, double valueValue)
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

public sealed record AdaptiveSkillProfileSnapshotWindowDaysOffsetAdjuster(double Weight) : IAdaptiveSkillProfileSnapshotStep
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(AdaptiveSkillProfileSnapshot item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record CapAdaptiveSkillProfileSnapshotAdjuster(double Cap) : IAdaptiveSkillProfileSnapshotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveSkillProfileSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentAdaptiveSkillProfileSnapshotAdjuster(double Percent) : IAdaptiveSkillProfileSnapshotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveSkillProfileSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelAdaptiveSkillProfileSnapshotAdjuster(Dictionary<string, double> Factors) : IAdaptiveSkillProfileSnapshotStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AdaptiveSkillProfileSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAdaptiveSkillProfileSnapshotAdjuster(double Minimum) : IAdaptiveSkillProfileSnapshotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveSkillProfileSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive skill profile snapshot steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveSkillProfileSnapshotEngine(List<IAdaptiveSkillProfileSnapshotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveSkillProfileSnapshotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveSkillProfileSnapshotEngine DefaultAdaptiveSkillProfileSnapshotEngine()
    {
        return new AdaptiveSkillProfileSnapshotEngine(
            new List<IAdaptiveSkillProfileSnapshotStep>
            {
                new TieredAdaptiveSkillProfileSnapshotAdjuster(
                    new List<double> { 972.0, 2078.0, 7433.0 },
                    new List<double> { 1.05, 1.02, 1.19 }),
                new AdaptiveSkillProfileSnapshotWindowDaysOffsetAdjuster(1.71),
                new CapAdaptiveSkillProfileSnapshotAdjuster(19708.0),
                new PercentAdaptiveSkillProfileSnapshotAdjuster(13.6),
                new LabelAdaptiveSkillProfileSnapshotAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.01, ["east"] = 1.0 }),
                new MinimumAdaptiveSkillProfileSnapshotAdjuster(23.6),
            },
            new List<string>(),
            0.8,
            81243.0);
    }

    public double Run(AdaptiveSkillProfileSnapshot item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveSkillProfileSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
