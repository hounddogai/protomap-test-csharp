namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate template calculation.</summary>
public interface IRateTemplateStep
{
    string Describe();
    double Adjust(RateTemplate item, double valueValue);
}

public sealed record RateTemplateWeightOffsetAdjuster(double Weight) : IRateTemplateStep
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(RateTemplate item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record RoundingRateTemplateAdjuster(double Step) : IRateTemplateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RateTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRateTemplateAdjuster(List<double> Bounds, List<double> Rates) : IRateTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateTemplate item, double valueValue)
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

public sealed record FlaggedRateTemplateAdjuster(Registry Flags, string Flag, double Percent) : IRateTemplateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RateTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rate template steps in order and keeps a trace of each.</summary>
public sealed class RateTemplateEngine(List<IRateTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateTemplateEngine DefaultRateTemplateEngine(Registry flags)
    {
        return new RateTemplateEngine(
            new List<IRateTemplateStep>
            {
                new RateTemplateWeightOffsetAdjuster(0.6),
                new RoundingRateTemplateAdjuster(1.0),
                new TieredRateTemplateAdjuster(
                    new List<double> { 866.0, 6010.0, 6881.0 },
                    new List<double> { 1.17, 1.12, 0.98 }),
                new FlaggedRateTemplateAdjuster(flags, "rate-template-scheduled", -5.3),
            },
            new List<string>(),
            1.9,
            93954.5);
    }

    public double Compute(RateTemplate item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RateTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
