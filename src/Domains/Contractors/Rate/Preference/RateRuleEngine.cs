namespace Acme.Workforce.Contractors.Rate.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate rule calculation.</summary>
public interface IRateRuleModifier
{
    string Describe();
    double Adjust(RateRule item, double valueValue);
}

public sealed record RoundingRateRuleAdjuster(double Step) : IRateRuleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RateRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedRateRuleAdjuster(Registry Flags, string Flag, double Percent) : IRateRuleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RateRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RateRuleRatioOffsetAdjuster(double Weight) : IRateRuleModifier
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(RateRule item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record PercentRateRuleAdjuster(double Percent) : IRateRuleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rate rule steps in order and keeps a trace of each.</summary>
public sealed class RateRuleEngine(List<IRateRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateRuleEngine DefaultRateRuleEngine(Registry flags)
    {
        return new RateRuleEngine(
            new List<IRateRuleModifier>
            {
                new RoundingRateRuleAdjuster(0.1),
                new FlaggedRateRuleAdjuster(flags, "rate-rule-legacy", 5.6),
                new RateRuleRatioOffsetAdjuster(1.65),
                new PercentRateRuleAdjuster(-24.7),
            },
            new List<string>(),
            1.6,
            7030.6);
    }

    public double Run(RateRule item)
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

    public Dictionary<string, double> EvaluateAll(List<RateRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
