namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling succession plan ledger calculation.</summary>
public interface IRollingSuccessionPlanLedgerFactor
{
    string Describe();
    double Adjust(RollingSuccessionPlanLedger item, double valueValue);
}

public sealed record FlaggedRollingSuccessionPlanLedgerAdjuster(Registry Flags, string Flag, double Percent) : IRollingSuccessionPlanLedgerFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingSuccessionPlanLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRollingSuccessionPlanLedgerAdjuster(double Percent) : IRollingSuccessionPlanLedgerFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingSuccessionPlanLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRollingSuccessionPlanLedgerAdjuster(double Step) : IRollingSuccessionPlanLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingSuccessionPlanLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRollingSuccessionPlanLedgerAdjuster(double Cap) : IRollingSuccessionPlanLedgerFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingSuccessionPlanLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRollingSuccessionPlanLedgerAdjuster(List<double> Bounds, List<double> Rates) : IRollingSuccessionPlanLedgerFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingSuccessionPlanLedger item, double valueValue)
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

/// <summary>Runs the rolling succession plan ledger steps in order and keeps a trace of each.</summary>
public sealed class RollingSuccessionPlanLedgerEngine(List<IRollingSuccessionPlanLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingSuccessionPlanLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingSuccessionPlanLedgerEngine DefaultRollingSuccessionPlanLedgerEngine(Registry flags)
    {
        return new RollingSuccessionPlanLedgerEngine(
            new List<IRollingSuccessionPlanLedgerFactor>
            {
                new FlaggedRollingSuccessionPlanLedgerAdjuster(flags, "rolling-succession-plan-ledger-tiered", 7.3),
                new PercentRollingSuccessionPlanLedgerAdjuster(21.8),
                new RoundingRollingSuccessionPlanLedgerAdjuster(0.1),
                new CapRollingSuccessionPlanLedgerAdjuster(24450.0),
                new TieredRollingSuccessionPlanLedgerAdjuster(
                    new List<double> { 812.0, 6861.0 },
                    new List<double> { 1.16, 0.98 }),
            },
            new List<string>(),
            2.4,
            70003.8);
    }

    public double Price(RollingSuccessionPlanLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingSuccessionPlanLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
