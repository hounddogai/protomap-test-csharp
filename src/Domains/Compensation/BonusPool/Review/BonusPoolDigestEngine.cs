namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool digest calculation.</summary>
public interface IBonusPoolDigestAdjuster
{
    string Describe();
    double Adjust(BonusPoolDigest item, double valueValue);
}

public sealed record TieredBonusPoolDigestAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolDigestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolDigest item, double valueValue)
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

public sealed record MinimumBonusPoolDigestAdjuster(double Minimum) : IBonusPoolDigestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameBonusPoolDigestAdjuster(Dictionary<string, double> Factors) : IBonusPoolDigestAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BonusPoolDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBonusPoolDigestAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BonusPoolDigestUpdatedAtOffsetAdjuster(double Weight) : IBonusPoolDigestAdjuster
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(BonusPoolDigest item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record RoundingBonusPoolDigestAdjuster(double Step) : IBonusPoolDigestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BonusPoolDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the bonus pool digest steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolDigestEngine(List<IBonusPoolDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolDigestEngine DefaultBonusPoolDigestEngine(Registry flags)
    {
        return new BonusPoolDigestEngine(
            new List<IBonusPoolDigestAdjuster>
            {
                new TieredBonusPoolDigestAdjuster(
                    new List<double> { 5992.0, 7773.0 },
                    new List<double> { 0.89, 0.97 }),
                new MinimumBonusPoolDigestAdjuster(29.5),
                new NameBonusPoolDigestAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.37, ["west"] = 1.32, ["retail"] = 1.11 }),
                new FlaggedBonusPoolDigestAdjuster(flags, "bonus-pool-digest-default", 2.1),
                new BonusPoolDigestUpdatedAtOffsetAdjuster(0.7),
                new RoundingBonusPoolDigestAdjuster(5.0),
            },
            new List<string>(),
            1.0,
            51884.7);
    }

    public double Compute(BonusPoolDigest item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
