namespace Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy offboarding request calculation.</summary>
public interface ILegacyOffboardingRequestAdjuster
{
    string Describe();
    double Adjust(LegacyOffboardingRequest item, double valueValue);
}

public sealed record RoundingLegacyOffboardingRequestAdjuster(double Step) : ILegacyOffboardingRequestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyOffboardingRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelLegacyOffboardingRequestAdjuster(Dictionary<string, double> Factors) : ILegacyOffboardingRequestAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LegacyOffboardingRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLegacyOffboardingRequestAdjuster(double Cap) : ILegacyOffboardingRequestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyOffboardingRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLegacyOffboardingRequestAdjuster(List<double> Bounds, List<double> Rates) : ILegacyOffboardingRequestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyOffboardingRequest item, double valueValue)
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

public sealed record LegacyOffboardingRequestExpiresAtOffsetAdjuster(double Weight) : ILegacyOffboardingRequestAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(LegacyOffboardingRequest item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record MinimumLegacyOffboardingRequestAdjuster(double Minimum) : ILegacyOffboardingRequestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyOffboardingRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy offboarding request steps in order and keeps a trace of each.</summary>
public sealed class LegacyOffboardingRequestEngine(List<ILegacyOffboardingRequestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyOffboardingRequestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyOffboardingRequestEngine DefaultLegacyOffboardingRequestEngine()
    {
        return new LegacyOffboardingRequestEngine(
            new List<ILegacyOffboardingRequestAdjuster>
            {
                new RoundingLegacyOffboardingRequestAdjuster(0.05),
                new LabelLegacyOffboardingRequestAdjuster(new Dictionary<string, double> { ["west"] = 1.34 }),
                new CapLegacyOffboardingRequestAdjuster(10829.0),
                new TieredLegacyOffboardingRequestAdjuster(
                    new List<double> { 1839.0, 2643.0, 8615.0 },
                    new List<double> { 1.05, 0.99, 0.98 }),
                new LegacyOffboardingRequestExpiresAtOffsetAdjuster(0.89),
                new MinimumLegacyOffboardingRequestAdjuster(18.1),
            },
            new List<string>(),
            0.9,
            28495.1);
    }

    public double Price(LegacyOffboardingRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyOffboardingRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
