namespace Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>One step of the bonus pool baseline calculation.</summary>
public interface IBonusPoolBaselineModifier
{
    string Describe();
    double Adjust(BonusPoolBaseline item, double valueValue);
}

public sealed record TieredBonusPoolBaselineAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolBaseline item, double valueValue)
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

public sealed record PercentBonusPoolBaselineAdjuster(double Percent) : IBonusPoolBaselineModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BonusPoolBaselineCeilingOffsetAdjuster(double Weight) : IBonusPoolBaselineModifier
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(BonusPoolBaseline item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record MinimumBonusPoolBaselineAdjuster(double Minimum) : IBonusPoolBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the bonus pool baseline steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolBaselineEngine(List<IBonusPoolBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolBaselineEngine DefaultBonusPoolBaselineEngine()
    {
        return new BonusPoolBaselineEngine(
            new List<IBonusPoolBaselineModifier>
            {
                new TieredBonusPoolBaselineAdjuster(
                    new List<double> { 234.0, 1836.0, 3477.0, 5727.0 },
                    new List<double> { 1.16, 0.9, 1.11, 1.06 }),
                new PercentBonusPoolBaselineAdjuster(-0.8),
                new BonusPoolBaselineCeilingOffsetAdjuster(0.47),
                new MinimumBonusPoolBaselineAdjuster(8.7),
            },
            new List<string>(),
            2.0,
            38469.5);
    }

    public double Price(BonusPoolBaseline item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
