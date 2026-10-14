namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual policy calculation.</summary>
public interface IAccrualPolicyAdjuster
{
    string Describe();
    double Adjust(AccrualPolicy item, double valueValue);
}

public sealed record FlaggedAccrualPolicyAdjuster(Registry Flags, string Flag, double Percent) : IAccrualPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccrualPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAccrualPolicyAdjuster(double Percent) : IAccrualPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAccrualPolicyAdjuster(List<double> Bounds, List<double> Rates) : IAccrualPolicyAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualPolicy item, double valueValue)
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

public sealed record CapAccrualPolicyAdjuster(double Cap) : IAccrualPolicyAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAccrualPolicyAdjuster(double Step) : IAccrualPolicyAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the accrual policy steps in order and keeps a trace of each.</summary>
public sealed class AccrualPolicyEngine(List<IAccrualPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualPolicyEngine DefaultAccrualPolicyEngine(Registry flags)
    {
        return new AccrualPolicyEngine(
            new List<IAccrualPolicyAdjuster>
            {
                new FlaggedAccrualPolicyAdjuster(flags, "accrual-policy-scheduled", -1.3),
                new PercentAccrualPolicyAdjuster(16.7),
                new TieredAccrualPolicyAdjuster(
                    new List<double> { 598.0, 2145.0, 8540.0, 9423.0 },
                    new List<double> { 1.07, 1.02, 0.85, 1.11 }),
                new CapAccrualPolicyAdjuster(29688.0),
                new RoundingAccrualPolicyAdjuster(0.05),
            },
            new List<string>(),
            4.0,
            70727.1);
    }

    public double Compute(AccrualPolicy item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
