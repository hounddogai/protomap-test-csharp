namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request ledger calculation.</summary>
public interface ILeaveRequestLedgerModifier
{
    string Describe();
    double Adjust(LeaveRequestLedger item, double valueValue);
}

public sealed record LeaveRequestLedgerExpiresAtOffsetAdjuster(double Weight) : ILeaveRequestLedgerModifier
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(LeaveRequestLedger item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record PercentLeaveRequestLedgerAdjuster(double Percent) : ILeaveRequestLedgerModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLeaveRequestLedgerAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestLedgerModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestLedger item, double valueValue)
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

public sealed record TeamLeaveRequestLedgerAdjuster(Dictionary<string, double> Factors) : ILeaveRequestLedgerModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(LeaveRequestLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLeaveRequestLedgerAdjuster(double Cap) : ILeaveRequestLedgerModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLeaveRequestLedgerAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestLedgerModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the leave request ledger steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestLedgerEngine(List<ILeaveRequestLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestLedgerEngine DefaultLeaveRequestLedgerEngine(Registry flags)
    {
        return new LeaveRequestLedgerEngine(
            new List<ILeaveRequestLedgerModifier>
            {
                new LeaveRequestLedgerExpiresAtOffsetAdjuster(1.53),
                new PercentLeaveRequestLedgerAdjuster(-5.8),
                new TieredLeaveRequestLedgerAdjuster(
                    new List<double> { 1448.0, 5160.0 },
                    new List<double> { 1.04, 0.93 }),
                new TeamLeaveRequestLedgerAdjuster(new Dictionary<string, double> { ["west"] = 0.75 }),
                new CapLeaveRequestLedgerAdjuster(42444.0),
                new FlaggedLeaveRequestLedgerAdjuster(flags, "leave-request-ledger-fallback", 2.3),
            },
            new List<string>(),
            0.9,
            24343.5);
    }

    public double Compute(LeaveRequestLedger item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
