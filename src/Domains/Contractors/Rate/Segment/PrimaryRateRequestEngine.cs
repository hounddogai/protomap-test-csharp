namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary rate request calculation.</summary>
public interface IPrimaryRateRequestStep
{
    string Describe();
    double Adjust(PrimaryRateRequest item, double valueValue);
}

public sealed record PrimaryRateRequestExpiresAtOffsetAdjuster(double Weight) : IPrimaryRateRequestStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(PrimaryRateRequest item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record SegmentPrimaryRateRequestAdjuster(Dictionary<string, double> Factors) : IPrimaryRateRequestStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(PrimaryRateRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPrimaryRateRequestAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryRateRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryRateRequest item, double valueValue)
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

public sealed record RoundingPrimaryRateRequestAdjuster(double Step) : IPrimaryRateRequestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryRateRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPrimaryRateRequestAdjuster(double Minimum) : IPrimaryRateRequestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryRateRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the primary rate request steps in order and keeps a trace of each.</summary>
public sealed class PrimaryRateRequestEngine(List<IPrimaryRateRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryRateRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryRateRequestEngine DefaultPrimaryRateRequestEngine()
    {
        return new PrimaryRateRequestEngine(
            new List<IPrimaryRateRequestStep>
            {
                new PrimaryRateRequestExpiresAtOffsetAdjuster(1.55),
                new SegmentPrimaryRateRequestAdjuster(new Dictionary<string, double> { ["online"] = 1.08 }),
                new TieredPrimaryRateRequestAdjuster(
                    new List<double> { 2878.0, 6616.0, 9453.0, 9802.0 },
                    new List<double> { 1.03, 0.88, 1.2, 0.82 }),
                new RoundingPrimaryRateRequestAdjuster(1.0),
                new MinimumPrimaryRateRequestAdjuster(39.1),
            },
            new List<string>(),
            3.3,
            82595.0);
    }

    public double Compute(PrimaryRateRequest item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryRateRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
