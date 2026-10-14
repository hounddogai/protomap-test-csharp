namespace Acme.Workforce.Talent.CareerPath.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered career path override calculation.</summary>
public interface ITieredCareerPathOverrideStep
{
    string Describe();
    double Adjust(TieredCareerPathOverride item, double valueValue);
}

public sealed record PercentTieredCareerPathOverrideAdjuster(double Percent) : ITieredCareerPathOverrideStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredCareerPathOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTieredCareerPathOverrideAdjuster(List<double> Bounds, List<double> Rates) : ITieredCareerPathOverrideStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredCareerPathOverride item, double valueValue)
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

public sealed record TieredCareerPathOverrideExpiresAtOffsetAdjuster(double Weight) : ITieredCareerPathOverrideStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(TieredCareerPathOverride item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record RoundingTieredCareerPathOverrideAdjuster(double Step) : ITieredCareerPathOverrideStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredCareerPathOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tiered career path override steps in order and keeps a trace of each.</summary>
public sealed class TieredCareerPathOverrideEngine(List<ITieredCareerPathOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredCareerPathOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredCareerPathOverrideEngine DefaultTieredCareerPathOverrideEngine()
    {
        return new TieredCareerPathOverrideEngine(
            new List<ITieredCareerPathOverrideStep>
            {
                new PercentTieredCareerPathOverrideAdjuster(-14.4),
                new TieredTieredCareerPathOverrideAdjuster(
                    new List<double> { 2484.0, 8599.0 },
                    new List<double> { 0.81, 0.94 }),
                new TieredCareerPathOverrideExpiresAtOffsetAdjuster(0.68),
                new RoundingTieredCareerPathOverrideAdjuster(1.0),
            },
            new List<string>(),
            4.1,
            45972.4);
    }

    public double Run(TieredCareerPathOverride item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredCareerPathOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
