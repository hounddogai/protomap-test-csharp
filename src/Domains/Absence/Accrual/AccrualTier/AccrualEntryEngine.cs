namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the accrual entry calculation.</summary>
public interface IAccrualEntryStep
{
    string Describe();
    double Adjust(AccrualEntry item, double valueValue);
}

public sealed record PercentAccrualEntryAdjuster(double Percent) : IAccrualEntryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAccrualEntryAdjuster(double Minimum) : IAccrualEntryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAccrualEntryAdjuster(Registry Flags, string Flag, double Percent) : IAccrualEntryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccrualEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAccrualEntryAdjuster(List<double> Bounds, List<double> Rates) : IAccrualEntryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualEntry item, double valueValue)
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

public sealed record AccrualEntryScoreOffsetAdjuster(double Weight) : IAccrualEntryStep
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(AccrualEntry item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the accrual entry steps in order and keeps a trace of each.</summary>
public sealed class AccrualEntryEngine(List<IAccrualEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualEntryEngine DefaultAccrualEntryEngine(Registry flags)
    {
        return new AccrualEntryEngine(
            new List<IAccrualEntryStep>
            {
                new PercentAccrualEntryAdjuster(-1.3),
                new MinimumAccrualEntryAdjuster(1.7),
                new FlaggedAccrualEntryAdjuster(flags, "accrual-entry-seasonal", -8.0),
                new TieredAccrualEntryAdjuster(
                    new List<double> { 4637.0, 5616.0, 9823.0 },
                    new List<double> { 0.94, 1.02, 0.96 }),
                new AccrualEntryScoreOffsetAdjuster(0.19),
            },
            new List<string>(),
            1.8,
            68896.5);
    }

    public double Price(AccrualEntry item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
