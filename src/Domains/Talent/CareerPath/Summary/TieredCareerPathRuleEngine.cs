namespace Acme.Workforce.Talent.CareerPath.Summary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tiered career path rule calculation.</summary>
public interface ITieredCareerPathRuleAdjuster
{
    string Describe();
    double Adjust(TieredCareerPathRule item, double valueValue);
}

public sealed record FlaggedTieredCareerPathRuleAdjuster(Registry Flags, string Flag, double Percent) : ITieredCareerPathRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredCareerPathRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTieredCareerPathRuleAdjuster(double Minimum) : ITieredCareerPathRuleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredCareerPathRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCareerPathRuleSequenceOffsetAdjuster(double Weight) : ITieredCareerPathRuleAdjuster
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(TieredCareerPathRule item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record CapTieredCareerPathRuleAdjuster(double Cap) : ITieredCareerPathRuleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredCareerPathRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the tiered career path rule steps in order and keeps a trace of each.</summary>
public sealed class TieredCareerPathRuleEngine(List<ITieredCareerPathRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredCareerPathRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredCareerPathRuleEngine DefaultTieredCareerPathRuleEngine(Registry flags)
    {
        return new TieredCareerPathRuleEngine(
            new List<ITieredCareerPathRuleAdjuster>
            {
                new FlaggedTieredCareerPathRuleAdjuster(flags, "tiered-career-path-rule-scheduled", 2.4),
                new MinimumTieredCareerPathRuleAdjuster(18.2),
                new TieredCareerPathRuleSequenceOffsetAdjuster(0.65),
                new CapTieredCareerPathRuleAdjuster(13688.0),
            },
            new List<string>(),
            3.3,
            71665.0);
    }

    public double Price(TieredCareerPathRule item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TieredCareerPathRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
