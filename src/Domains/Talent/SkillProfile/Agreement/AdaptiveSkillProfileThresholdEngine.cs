namespace Acme.Workforce.Talent.SkillProfile.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive skill profile threshold calculation.</summary>
public interface IAdaptiveSkillProfileThresholdFactor
{
    string Describe();
    double Adjust(AdaptiveSkillProfileThreshold item, double valueValue);
}

public sealed record MinimumAdaptiveSkillProfileThresholdAdjuster(double Minimum) : IAdaptiveSkillProfileThresholdFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveSkillProfileThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAdaptiveSkillProfileThresholdAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveSkillProfileThresholdFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveSkillProfileThreshold item, double valueValue)
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

public sealed record RoundingAdaptiveSkillProfileThresholdAdjuster(double Step) : IAdaptiveSkillProfileThresholdFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveSkillProfileThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleAdaptiveSkillProfileThresholdAdjuster(Dictionary<string, double> Factors) : IAdaptiveSkillProfileThresholdFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AdaptiveSkillProfileThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAdaptiveSkillProfileThresholdAdjuster(double Cap) : IAdaptiveSkillProfileThresholdFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveSkillProfileThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AdaptiveSkillProfileThresholdBatchSizeOffsetAdjuster(double Weight) : IAdaptiveSkillProfileThresholdFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(AdaptiveSkillProfileThreshold item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

/// <summary>Runs the adaptive skill profile threshold steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveSkillProfileThresholdEngine(List<IAdaptiveSkillProfileThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveSkillProfileThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveSkillProfileThresholdEngine DefaultAdaptiveSkillProfileThresholdEngine()
    {
        return new AdaptiveSkillProfileThresholdEngine(
            new List<IAdaptiveSkillProfileThresholdFactor>
            {
                new MinimumAdaptiveSkillProfileThresholdAdjuster(30.6),
                new TieredAdaptiveSkillProfileThresholdAdjuster(
                    new List<double> { 2400.0, 2443.0 },
                    new List<double> { 0.87, 1.04 }),
                new RoundingAdaptiveSkillProfileThresholdAdjuster(1.0),
                new TitleAdaptiveSkillProfileThresholdAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.93, ["west"] = 1.21, ["retail"] = 1.29 }),
                new CapAdaptiveSkillProfileThresholdAdjuster(14279.0),
                new AdaptiveSkillProfileThresholdBatchSizeOffsetAdjuster(1.67),
            },
            new List<string>(),
            1.9,
            90421.5);
    }

    public double Price(AdaptiveSkillProfileThreshold item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveSkillProfileThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
