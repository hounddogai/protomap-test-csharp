namespace Acme.Workforce.Absence.Accrual.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual snapshot calculation.</summary>
public interface IAccrualSnapshotStep
{
    string Describe();
    double Adjust(AccrualSnapshot item, double valueValue);
}

public sealed record RoundingAccrualSnapshotAdjuster(double Step) : IAccrualSnapshotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAccrualSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IAccrualSnapshotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualSnapshot item, double valueValue)
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

public sealed record PercentAccrualSnapshotAdjuster(double Percent) : IAccrualSnapshotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the accrual snapshot steps in order and keeps a trace of each.</summary>
public sealed class AccrualSnapshotEngine(List<IAccrualSnapshotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualSnapshotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualSnapshotEngine DefaultAccrualSnapshotEngine()
    {
        return new AccrualSnapshotEngine(
            new List<IAccrualSnapshotStep>
            {
                new RoundingAccrualSnapshotAdjuster(1.0),
                new TieredAccrualSnapshotAdjuster(
                    new List<double> { 1019.0, 3753.0, 8176.0 },
                    new List<double> { 1.11, 1.08, 1.03 }),
                new PercentAccrualSnapshotAdjuster(21.6),
            },
            new List<string>(),
            1.6,
            48646.6);
    }

    public double Compute(AccrualSnapshot item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
