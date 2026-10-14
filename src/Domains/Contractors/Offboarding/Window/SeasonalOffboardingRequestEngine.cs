namespace Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>One step of the seasonal offboarding request calculation.</summary>
public interface ISeasonalOffboardingRequestModifier
{
    string Describe();
    double Adjust(SeasonalOffboardingRequest item, double valueValue);
}

public sealed record TieredSeasonalOffboardingRequestAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalOffboardingRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalOffboardingRequest item, double valueValue)
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

public sealed record LabelSeasonalOffboardingRequestAdjuster(Dictionary<string, double> Factors) : ISeasonalOffboardingRequestModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SeasonalOffboardingRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record SeasonalOffboardingRequestCostOffsetAdjuster(double Weight) : ISeasonalOffboardingRequestModifier
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(SeasonalOffboardingRequest item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record MinimumSeasonalOffboardingRequestAdjuster(double Minimum) : ISeasonalOffboardingRequestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalOffboardingRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentSeasonalOffboardingRequestAdjuster(double Percent) : ISeasonalOffboardingRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalOffboardingRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalOffboardingRequestAdjuster(double Cap) : ISeasonalOffboardingRequestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalOffboardingRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the seasonal offboarding request steps in order and keeps a trace of each.</summary>
public sealed class SeasonalOffboardingRequestEngine(List<ISeasonalOffboardingRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalOffboardingRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalOffboardingRequestEngine DefaultSeasonalOffboardingRequestEngine()
    {
        return new SeasonalOffboardingRequestEngine(
            new List<ISeasonalOffboardingRequestModifier>
            {
                new TieredSeasonalOffboardingRequestAdjuster(
                    new List<double> { 7616.0, 8220.0 },
                    new List<double> { 1.02, 0.93 }),
                new LabelSeasonalOffboardingRequestAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.87, ["retail"] = 0.77, ["east"] = 0.81 }),
                new SeasonalOffboardingRequestCostOffsetAdjuster(0.99),
                new MinimumSeasonalOffboardingRequestAdjuster(33.4),
                new PercentSeasonalOffboardingRequestAdjuster(6.0),
                new CapSeasonalOffboardingRequestAdjuster(48469.0),
            },
            new List<string>(),
            0.1,
            78539.7);
    }

    public double Compute(SeasonalOffboardingRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalOffboardingRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
