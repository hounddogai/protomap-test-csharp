namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive leave request ledger calculation.</summary>
public interface IAdaptiveLeaveRequestLedgerFactor
{
    string Describe();
    double Adjust(AdaptiveLeaveRequestLedger item, double valueValue);
}

public sealed record DepartmentAdaptiveLeaveRequestLedgerAdjuster(Dictionary<string, double> Factors) : IAdaptiveLeaveRequestLedgerFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(AdaptiveLeaveRequestLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AdaptiveLeaveRequestLedgerQuantityOffsetAdjuster(double Weight) : IAdaptiveLeaveRequestLedgerFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(AdaptiveLeaveRequestLedger item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record TieredAdaptiveLeaveRequestLedgerAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveLeaveRequestLedgerFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveLeaveRequestLedger item, double valueValue)
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

public sealed record MinimumAdaptiveLeaveRequestLedgerAdjuster(double Minimum) : IAdaptiveLeaveRequestLedgerFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveLeaveRequestLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive leave request ledger steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveLeaveRequestLedgerEngine(List<IAdaptiveLeaveRequestLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveLeaveRequestLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveLeaveRequestLedgerEngine DefaultAdaptiveLeaveRequestLedgerEngine()
    {
        return new AdaptiveLeaveRequestLedgerEngine(
            new List<IAdaptiveLeaveRequestLedgerFactor>
            {
                new DepartmentAdaptiveLeaveRequestLedgerAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.8, ["south"] = 0.96 }),
                new AdaptiveLeaveRequestLedgerQuantityOffsetAdjuster(0.76),
                new TieredAdaptiveLeaveRequestLedgerAdjuster(
                    new List<double> { 5207.0, 7435.0 },
                    new List<double> { 1.07, 0.92 }),
                new MinimumAdaptiveLeaveRequestLedgerAdjuster(40.9),
            },
            new List<string>(),
            3.7,
            76325.7);
    }

    public double Evaluate(AdaptiveLeaveRequestLedger item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveLeaveRequestLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
