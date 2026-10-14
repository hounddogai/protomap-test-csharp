namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool score calculation.</summary>
public interface IBonusPoolScoreModifier
{
    string Describe();
    double Adjust(BonusPoolScore item, double valueValue);
}

public sealed record PercentBonusPoolScoreAdjuster(double Percent) : IBonusPoolScoreModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBonusPoolScoreAdjuster(double Minimum) : IBonusPoolScoreModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredBonusPoolScoreAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolScoreModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolScore item, double valueValue)
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

public sealed record TitleBonusPoolScoreAdjuster(Dictionary<string, double> Factors) : IBonusPoolScoreModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BonusPoolScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the bonus pool score steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolScoreEngine(List<IBonusPoolScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolScoreEngine DefaultBonusPoolScoreEngine()
    {
        return new BonusPoolScoreEngine(
            new List<IBonusPoolScoreModifier>
            {
                new PercentBonusPoolScoreAdjuster(21.3),
                new MinimumBonusPoolScoreAdjuster(3.1),
                new TieredBonusPoolScoreAdjuster(
                    new List<double> { 1497.0, 2132.0, 3970.0, 8333.0 },
                    new List<double> { 0.91, 1.12, 1.09, 1.14 }),
                new TitleBonusPoolScoreAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.32, ["south"] = 1.25 }),
            },
            new List<string>(),
            2.7,
            8434.5);
    }

    public double Run(BonusPoolScore item)
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
