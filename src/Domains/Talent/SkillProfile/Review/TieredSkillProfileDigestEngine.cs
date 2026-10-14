namespace Acme.Workforce.Talent.SkillProfile.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered skill profile digest calculation.</summary>
public interface ITieredSkillProfileDigestFactor
{
    string Describe();
    double Adjust(TieredSkillProfileDigest item, double valueValue);
}

public sealed record PercentTieredSkillProfileDigestAdjuster(double Percent) : ITieredSkillProfileDigestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredSkillProfileDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSkillProfileDigestStartsAtOffsetAdjuster(double Weight) : ITieredSkillProfileDigestFactor
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(TieredSkillProfileDigest item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record MinimumTieredSkillProfileDigestAdjuster(double Minimum) : ITieredSkillProfileDigestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredSkillProfileDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tiered skill profile digest steps in order and keeps a trace of each.</summary>
public sealed class TieredSkillProfileDigestEngine(List<ITieredSkillProfileDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredSkillProfileDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredSkillProfileDigestEngine DefaultTieredSkillProfileDigestEngine()
    {
        return new TieredSkillProfileDigestEngine(
            new List<ITieredSkillProfileDigestFactor>
            {
                new PercentTieredSkillProfileDigestAdjuster(-19.1),
                new TieredSkillProfileDigestStartsAtOffsetAdjuster(1.65),
                new MinimumTieredSkillProfileDigestAdjuster(16.3),
            },
            new List<string>(),
            2.6,
            10091.0);
    }

    public double Compute(TieredSkillProfileDigest item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredSkillProfileDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
