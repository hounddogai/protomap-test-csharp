namespace Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>One step of the bonus pool entry calculation.</summary>
public interface IBonusPoolEntryStep
{
    string Describe();
    double Adjust(BonusPoolEntry item, double valueValue);
}

public sealed record TieredBonusPoolEntryAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolEntryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolEntry item, double valueValue)
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

public sealed record PercentBonusPoolEntryAdjuster(double Percent) : IBonusPoolEntryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BonusPoolEntryTenureMonthsOffsetAdjuster(double Weight) : IBonusPoolEntryStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(BonusPoolEntry item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

/// <summary>Runs the bonus pool entry steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolEntryEngine(List<IBonusPoolEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolEntryEngine DefaultBonusPoolEntryEngine()
    {
        return new BonusPoolEntryEngine(
            new List<IBonusPoolEntryStep>
            {
                new TieredBonusPoolEntryAdjuster(
                    new List<double> { 1729.0, 2809.0, 6690.0, 9943.0 },
                    new List<double> { 1.11, 1.13, 0.91, 1.1 }),
                new PercentBonusPoolEntryAdjuster(-23.2),
                new BonusPoolEntryTenureMonthsOffsetAdjuster(1.92),
            },
            new List<string>(),
            2.5,
            69496.9);
    }

    public double Run(BonusPoolEntry item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
