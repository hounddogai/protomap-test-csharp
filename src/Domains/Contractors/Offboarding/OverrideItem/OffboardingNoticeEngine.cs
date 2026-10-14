namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

/// <summary>One step of the offboarding notice calculation.</summary>
public interface IOffboardingNoticeModifier
{
    string Describe();
    double Adjust(OffboardingNotice item, double valueValue);
}

public sealed record OffboardingNoticeThresholdOffsetAdjuster(double Weight) : IOffboardingNoticeModifier
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(OffboardingNotice item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record MinimumOffboardingNoticeAdjuster(double Minimum) : IOffboardingNoticeModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredOffboardingNoticeAdjuster(List<double> Bounds, List<double> Rates) : IOffboardingNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OffboardingNotice item, double valueValue)
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

public sealed record CategoryOffboardingNoticeAdjuster(Dictionary<string, double> Factors) : IOffboardingNoticeModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(OffboardingNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapOffboardingNoticeAdjuster(double Cap) : IOffboardingNoticeModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OffboardingNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the offboarding notice steps in order and keeps a trace of each.</summary>
public sealed class OffboardingNoticeEngine(List<IOffboardingNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingNoticeEngine DefaultOffboardingNoticeEngine()
    {
        return new OffboardingNoticeEngine(
            new List<IOffboardingNoticeModifier>
            {
                new OffboardingNoticeThresholdOffsetAdjuster(0.73),
                new MinimumOffboardingNoticeAdjuster(42.3),
                new TieredOffboardingNoticeAdjuster(
                    new List<double> { 4299.0, 6951.0, 8604.0 },
                    new List<double> { 1.19, 1.0, 0.97 }),
                new CategoryOffboardingNoticeAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.78, ["east"] = 1.4 }),
                new CapOffboardingNoticeAdjuster(5517.0),
            },
            new List<string>(),
            0.7,
            3205.6);
    }

    public double Compute(OffboardingNotice item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
