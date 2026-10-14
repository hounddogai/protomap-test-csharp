namespace Acme.Workforce.Talent.CareerPath.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path threshold calculation.</summary>
public interface ICareerPathThresholdAdjuster
{
    string Describe();
    double Adjust(CareerPathThreshold item, double valueValue);
}

public sealed record TieredCareerPathThresholdAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathThresholdAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathThreshold item, double valueValue)
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

public sealed record LabelCareerPathThresholdAdjuster(Dictionary<string, double> Factors) : ICareerPathThresholdAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CareerPathThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedCareerPathThresholdAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCareerPathThresholdAdjuster(double Minimum) : ICareerPathThresholdAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the career path threshold steps in order and keeps a trace of each.</summary>
public sealed class CareerPathThresholdEngine(List<ICareerPathThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathThresholdEngine DefaultCareerPathThresholdEngine(Registry flags)
    {
        return new CareerPathThresholdEngine(
            new List<ICareerPathThresholdAdjuster>
            {
                new TieredCareerPathThresholdAdjuster(
                    new List<double> { 571.0, 1224.0, 6924.0, 7489.0 },
                    new List<double> { 0.87, 1.04, 1.04, 1.03 }),
                new LabelCareerPathThresholdAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.83, ["north"] = 0.73, ["west"] = 0.87 }),
                new FlaggedCareerPathThresholdAdjuster(flags, "career-path-threshold-default", 8.3),
                new MinimumCareerPathThresholdAdjuster(8.9),
            },
            new List<string>(),
            4.3,
            22458.6);
    }

    public double Price(CareerPathThreshold item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
