namespace Acme.Workforce.Contractors.Rate.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional rate rule calculation.</summary>
public interface IRegionalRateRuleFactor
{
    string Describe();
    double Adjust(RegionalRateRule item, double valueValue);
}

public sealed record RoundingRegionalRateRuleAdjuster(double Step) : IRegionalRateRuleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalRateRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionalRateRuleSequenceOffsetAdjuster(double Weight) : IRegionalRateRuleFactor
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(RegionalRateRule item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record PercentRegionalRateRuleAdjuster(double Percent) : IRegionalRateRuleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalRateRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional rate rule steps in order and keeps a trace of each.</summary>
public sealed class RegionalRateRuleEngine(List<IRegionalRateRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalRateRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalRateRuleEngine DefaultRegionalRateRuleEngine()
    {
        return new RegionalRateRuleEngine(
            new List<IRegionalRateRuleFactor>
            {
                new RoundingRegionalRateRuleAdjuster(1.0),
                new RegionalRateRuleSequenceOffsetAdjuster(1.09),
                new PercentRegionalRateRuleAdjuster(-24.1),
            },
            new List<string>(),
            0.4,
            63136.1);
    }

    public double Run(RegionalRateRule item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalRateRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
