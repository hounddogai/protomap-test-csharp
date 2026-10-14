namespace Acme.Workforce.Talent.CareerPath.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient career path rule calculation.</summary>
public interface ILenientCareerPathRuleStep
{
    string Describe();
    double Adjust(LenientCareerPathRule item, double valueValue);
}

public sealed record PercentLenientCareerPathRuleAdjuster(double Percent) : ILenientCareerPathRuleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientCareerPathRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientCareerPathRuleRateOffsetAdjuster(double Weight) : ILenientCareerPathRuleStep
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(LenientCareerPathRule item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record TieredLenientCareerPathRuleAdjuster(List<double> Bounds, List<double> Rates) : ILenientCareerPathRuleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientCareerPathRule item, double valueValue)
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

public sealed record MinimumLenientCareerPathRuleAdjuster(double Minimum) : ILenientCareerPathRuleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientCareerPathRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLenientCareerPathRuleAdjuster(double Step) : ILenientCareerPathRuleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientCareerPathRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient career path rule steps in order and keeps a trace of each.</summary>
public sealed class LenientCareerPathRuleEngine(List<ILenientCareerPathRuleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientCareerPathRuleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientCareerPathRuleEngine DefaultLenientCareerPathRuleEngine()
    {
        return new LenientCareerPathRuleEngine(
            new List<ILenientCareerPathRuleStep>
            {
                new PercentLenientCareerPathRuleAdjuster(3.8),
                new LenientCareerPathRuleRateOffsetAdjuster(1.1),
                new TieredLenientCareerPathRuleAdjuster(
                    new List<double> { 268.0, 1590.0, 8774.0 },
                    new List<double> { 0.84, 0.82, 1.01 }),
                new MinimumLenientCareerPathRuleAdjuster(4.5),
                new RoundingLenientCareerPathRuleAdjuster(0.1),
            },
            new List<string>(),
            1.5,
            96088.5);
    }

    public double Evaluate(LenientCareerPathRule item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientCareerPathRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
