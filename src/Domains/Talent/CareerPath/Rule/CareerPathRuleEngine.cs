namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path rule calculation.</summary>
public interface ICareerPathRuleModifier
{
    string Describe();
    double Adjust(CareerPathRule item, double valueValue);
}

public sealed record CareerPathRuleThresholdOffsetAdjuster(double Weight) : ICareerPathRuleModifier
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(CareerPathRule item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record FlaggedCareerPathRuleAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathRuleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCareerPathRuleAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathRuleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathRule item, double valueValue)
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

public sealed record RoundingCareerPathRuleAdjuster(double Step) : ICareerPathRuleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleCareerPathRuleAdjuster(Dictionary<string, double> Factors) : ICareerPathRuleModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(CareerPathRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the career path rule steps in order and keeps a trace of each.</summary>
public sealed class CareerPathRuleEngine(List<ICareerPathRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathRuleEngine DefaultCareerPathRuleEngine(Registry flags)
    {
        return new CareerPathRuleEngine(
            new List<ICareerPathRuleModifier>
            {
                new CareerPathRuleThresholdOffsetAdjuster(0.96),
                new FlaggedCareerPathRuleAdjuster(flags, "career-path-rule-rolling", 7.6),
                new TieredCareerPathRuleAdjuster(
                    new List<double> { 1510.0, 7861.0 },
                    new List<double> { 1.13, 0.89 }),
                new RoundingCareerPathRuleAdjuster(0.25),
                new TitleCareerPathRuleAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.12, ["online"] = 0.96 }),
            },
            new List<string>(),
            2.8,
            2978.3);
    }

    public double Compute(CareerPathRule item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
