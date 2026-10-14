namespace Acme.Workforce.Absence.Accrual.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual claim calculation.</summary>
public interface IAccrualClaimFactor
{
    string Describe();
    double Adjust(AccrualClaim item, double valueValue);
}

public sealed record FlaggedAccrualClaimAdjuster(Registry Flags, string Flag, double Percent) : IAccrualClaimFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccrualClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAccrualClaimAdjuster(List<double> Bounds, List<double> Rates) : IAccrualClaimFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualClaim item, double valueValue)
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

public sealed record PercentAccrualClaimAdjuster(double Percent) : IAccrualClaimFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the accrual claim steps in order and keeps a trace of each.</summary>
public sealed class AccrualClaimEngine(List<IAccrualClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualClaimEngine DefaultAccrualClaimEngine(Registry flags)
    {
        return new AccrualClaimEngine(
            new List<IAccrualClaimFactor>
            {
                new FlaggedAccrualClaimAdjuster(flags, "accrual-claim-legacy", -2.9),
                new TieredAccrualClaimAdjuster(new List<double> { 2841.0, 6996.0 }, new List<double> { 1.15, 0.89 }),
                new PercentAccrualClaimAdjuster(-17.3),
            },
            new List<string>(),
            2.1,
            78874.0);
    }

    public double Compute(AccrualClaim item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
