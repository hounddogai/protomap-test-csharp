namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool profile calculation.</summary>
public interface IBonusPoolProfileFactor
{
    string Describe();
    double Adjust(BonusPoolProfile item, double valueValue);
}

public sealed record FlaggedBonusPoolProfileAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolProfileFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBonusPoolProfileAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolProfileFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolProfile item, double valueValue)
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

public sealed record RoundingBonusPoolProfileAdjuster(double Step) : IBonusPoolProfileFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BonusPoolProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBonusPoolProfileAdjuster(double Percent) : IBonusPoolProfileFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the bonus pool profile steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolProfileEngine(List<IBonusPoolProfileFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolProfileFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolProfileEngine DefaultBonusPoolProfileEngine(Registry flags)
    {
        return new BonusPoolProfileEngine(
            new List<IBonusPoolProfileFactor>
            {
                new FlaggedBonusPoolProfileAdjuster(flags, "bonus-pool-profile-primary", 3.6),
                new TieredBonusPoolProfileAdjuster(
                    new List<double> { 5365.0, 5814.0, 7750.0 },
                    new List<double> { 1.15, 0.8, 0.87 }),
                new RoundingBonusPoolProfileAdjuster(0.25),
                new PercentBonusPoolProfileAdjuster(21.3),
            },
            new List<string>(),
            2.4,
            64155.5);
    }

    public double Compute(BonusPoolProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
