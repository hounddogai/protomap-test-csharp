namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback access badge override calculation.</summary>
public interface IFallbackAccessBadgeOverrideStep
{
    string Describe();
    double Adjust(FallbackAccessBadgeOverride item, double valueValue);
}

public sealed record FallbackAccessBadgeOverrideRetriesOffsetAdjuster(double Weight) : IFallbackAccessBadgeOverrideStep
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(FallbackAccessBadgeOverride item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record RoundingFallbackAccessBadgeOverrideAdjuster(double Step) : IFallbackAccessBadgeOverrideStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackAccessBadgeOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredFallbackAccessBadgeOverrideAdjuster(List<double> Bounds, List<double> Rates) : IFallbackAccessBadgeOverrideStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackAccessBadgeOverride item, double valueValue)
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

public sealed record CapFallbackAccessBadgeOverrideAdjuster(double Cap) : IFallbackAccessBadgeOverrideStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackAccessBadgeOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CategoryFallbackAccessBadgeOverrideAdjuster(Dictionary<string, double> Factors) : IFallbackAccessBadgeOverrideStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(FallbackAccessBadgeOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumFallbackAccessBadgeOverrideAdjuster(double Minimum) : IFallbackAccessBadgeOverrideStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackAccessBadgeOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the fallback access badge override steps in order and keeps a trace of each.</summary>
public sealed class FallbackAccessBadgeOverrideEngine(List<IFallbackAccessBadgeOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackAccessBadgeOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackAccessBadgeOverrideEngine DefaultFallbackAccessBadgeOverrideEngine()
    {
        return new FallbackAccessBadgeOverrideEngine(
            new List<IFallbackAccessBadgeOverrideStep>
            {
                new FallbackAccessBadgeOverrideRetriesOffsetAdjuster(0.14),
                new RoundingFallbackAccessBadgeOverrideAdjuster(0.5),
                new TieredFallbackAccessBadgeOverrideAdjuster(
                    new List<double> { 3968.0, 4163.0, 8111.0 },
                    new List<double> { 0.85, 1.16, 0.99 }),
                new CapFallbackAccessBadgeOverrideAdjuster(33579.0),
                new CategoryFallbackAccessBadgeOverrideAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.83, ["west"] = 1.33, ["retail"] = 1.22 }),
                new MinimumFallbackAccessBadgeOverrideAdjuster(21.4),
            },
            new List<string>(),
            3.7,
            65399.5);
    }

    public double Evaluate(FallbackAccessBadgeOverride item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackAccessBadgeOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
