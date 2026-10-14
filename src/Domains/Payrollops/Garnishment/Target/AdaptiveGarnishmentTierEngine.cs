namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive garnishment tier calculation.</summary>
public interface IAdaptiveGarnishmentTierStep
{
    string Describe();
    double Adjust(AdaptiveGarnishmentTier item, double valueValue);
}

public sealed record MinimumAdaptiveGarnishmentTierAdjuster(double Minimum) : IAdaptiveGarnishmentTierStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveGarnishmentTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAdaptiveGarnishmentTierAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveGarnishmentTierStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveGarnishmentTier item, double valueValue)
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

public sealed record NameAdaptiveGarnishmentTierAdjuster(Dictionary<string, double> Factors) : IAdaptiveGarnishmentTierStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AdaptiveGarnishmentTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAdaptiveGarnishmentTierAdjuster(double Percent) : IAdaptiveGarnishmentTierStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveGarnishmentTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdaptiveGarnishmentTierAdjuster(double Cap) : IAdaptiveGarnishmentTierStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveGarnishmentTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAdaptiveGarnishmentTierAdjuster(double Step) : IAdaptiveGarnishmentTierStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveGarnishmentTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive garnishment tier steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveGarnishmentTierEngine(List<IAdaptiveGarnishmentTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveGarnishmentTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveGarnishmentTierEngine DefaultAdaptiveGarnishmentTierEngine()
    {
        return new AdaptiveGarnishmentTierEngine(
            new List<IAdaptiveGarnishmentTierStep>
            {
                new MinimumAdaptiveGarnishmentTierAdjuster(43.5),
                new TieredAdaptiveGarnishmentTierAdjuster(
                    new List<double> { 4799.0, 6014.0, 8085.0, 9123.0 },
                    new List<double> { 0.82, 1.0, 1.09, 1.04 }),
                new NameAdaptiveGarnishmentTierAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.74, ["south"] = 0.84, ["retail"] = 0.78 }),
                new PercentAdaptiveGarnishmentTierAdjuster(-8.8),
                new CapAdaptiveGarnishmentTierAdjuster(26225.0),
                new RoundingAdaptiveGarnishmentTierAdjuster(0.1),
            },
            new List<string>(),
            4.9,
            61865.0);
    }

    public double Price(AdaptiveGarnishmentTier item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveGarnishmentTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
