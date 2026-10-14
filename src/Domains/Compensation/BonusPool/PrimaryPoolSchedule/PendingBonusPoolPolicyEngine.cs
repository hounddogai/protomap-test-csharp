namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending bonus pool policy calculation.</summary>
public interface IPendingBonusPoolPolicyModifier
{
    string Describe();
    double Adjust(PendingBonusPoolPolicy item, double valueValue);
}

public sealed record PercentPendingBonusPoolPolicyAdjuster(double Percent) : IPendingBonusPoolPolicyModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingBonusPoolPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPendingBonusPoolPolicyAdjuster(double Cap) : IPendingBonusPoolPolicyModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingBonusPoolPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPendingBonusPoolPolicyAdjuster(List<double> Bounds, List<double> Rates) : IPendingBonusPoolPolicyModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingBonusPoolPolicy item, double valueValue)
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

public sealed record FlaggedPendingBonusPoolPolicyAdjuster(Registry Flags, string Flag, double Percent) : IPendingBonusPoolPolicyModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingBonusPoolPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitPendingBonusPoolPolicyAdjuster(Dictionary<string, double> Factors) : IPendingBonusPoolPolicyModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(PendingBonusPoolPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pending bonus pool policy steps in order and keeps a trace of each.</summary>
public sealed class PendingBonusPoolPolicyEngine(List<IPendingBonusPoolPolicyModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingBonusPoolPolicyModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingBonusPoolPolicyEngine DefaultPendingBonusPoolPolicyEngine(Registry flags)
    {
        return new PendingBonusPoolPolicyEngine(
            new List<IPendingBonusPoolPolicyModifier>
            {
                new PercentPendingBonusPoolPolicyAdjuster(4.7),
                new CapPendingBonusPoolPolicyAdjuster(36802.0),
                new TieredPendingBonusPoolPolicyAdjuster(
                    new List<double> { 134.0, 1809.0 },
                    new List<double> { 0.99, 1.15 }),
                new FlaggedPendingBonusPoolPolicyAdjuster(flags, "pending-bonus-pool-policy-seasonal", -2.1),
                new UnitPendingBonusPoolPolicyAdjuster(new Dictionary<string, double> { ["east"] = 0.98 }),
            },
            new List<string>(),
            4.7,
            30913.1);
    }

    public double Evaluate(PendingBonusPoolPolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingBonusPoolPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
