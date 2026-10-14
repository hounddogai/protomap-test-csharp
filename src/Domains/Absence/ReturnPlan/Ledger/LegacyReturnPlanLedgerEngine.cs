namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy return plan ledger calculation.</summary>
public interface ILegacyReturnPlanLedgerAdjuster
{
    string Describe();
    double Adjust(LegacyReturnPlanLedger item, double valueValue);
}

public sealed record UnitLegacyReturnPlanLedgerAdjuster(Dictionary<string, double> Factors) : ILegacyReturnPlanLedgerAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(LegacyReturnPlanLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLegacyReturnPlanLedgerAdjuster(List<double> Bounds, List<double> Rates) : ILegacyReturnPlanLedgerAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyReturnPlanLedger item, double valueValue)
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

public sealed record MinimumLegacyReturnPlanLedgerAdjuster(double Minimum) : ILegacyReturnPlanLedgerAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyReturnPlanLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLegacyReturnPlanLedgerAdjuster(double Cap) : ILegacyReturnPlanLedgerAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyReturnPlanLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LegacyReturnPlanLedgerTenureMonthsOffsetAdjuster(double Weight) : ILegacyReturnPlanLedgerAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(LegacyReturnPlanLedger item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

/// <summary>Runs the legacy return plan ledger steps in order and keeps a trace of each.</summary>
public sealed class LegacyReturnPlanLedgerEngine(List<ILegacyReturnPlanLedgerAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyReturnPlanLedgerAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyReturnPlanLedgerEngine DefaultLegacyReturnPlanLedgerEngine()
    {
        return new LegacyReturnPlanLedgerEngine(
            new List<ILegacyReturnPlanLedgerAdjuster>
            {
                new UnitLegacyReturnPlanLedgerAdjuster(new Dictionary<string, double> { ["retail"] = 1.31 }),
                new TieredLegacyReturnPlanLedgerAdjuster(
                    new List<double> { 768.0, 4187.0, 5106.0, 9615.0 },
                    new List<double> { 0.87, 0.94, 1.0, 1.0 }),
                new MinimumLegacyReturnPlanLedgerAdjuster(11.8),
                new CapLegacyReturnPlanLedgerAdjuster(9119.0),
                new LegacyReturnPlanLedgerTenureMonthsOffsetAdjuster(1.65),
            },
            new List<string>(),
            0.4,
            36240.3);
    }

    public double Evaluate(LegacyReturnPlanLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyReturnPlanLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
