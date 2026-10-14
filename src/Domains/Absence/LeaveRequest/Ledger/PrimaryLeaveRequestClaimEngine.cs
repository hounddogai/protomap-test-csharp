namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the primary leave request claim calculation.</summary>
public interface IPrimaryLeaveRequestClaimFactor
{
    string Describe();
    double Adjust(PrimaryLeaveRequestClaim item, double valueValue);
}

public sealed record FlaggedPrimaryLeaveRequestClaimAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryLeaveRequestClaimFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryLeaveRequestClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NamePrimaryLeaveRequestClaimAdjuster(Dictionary<string, double> Factors) : IPrimaryLeaveRequestClaimFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PrimaryLeaveRequestClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPrimaryLeaveRequestClaimAdjuster(double Cap) : IPrimaryLeaveRequestClaimFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryLeaveRequestClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PrimaryLeaveRequestClaimLeaveDaysOffsetAdjuster(double Weight) : IPrimaryLeaveRequestClaimFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(PrimaryLeaveRequestClaim item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

/// <summary>Runs the primary leave request claim steps in order and keeps a trace of each.</summary>
public sealed class PrimaryLeaveRequestClaimEngine(List<IPrimaryLeaveRequestClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryLeaveRequestClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryLeaveRequestClaimEngine DefaultPrimaryLeaveRequestClaimEngine(Registry flags)
    {
        return new PrimaryLeaveRequestClaimEngine(
            new List<IPrimaryLeaveRequestClaimFactor>
            {
                new FlaggedPrimaryLeaveRequestClaimAdjuster(flags, "primary-leave-request-claim-fallback", 9.1),
                new NamePrimaryLeaveRequestClaimAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.35, ["south"] = 1.09, ["west"] = 1.31 }),
                new CapPrimaryLeaveRequestClaimAdjuster(16691.0),
                new PrimaryLeaveRequestClaimLeaveDaysOffsetAdjuster(1.9),
            },
            new List<string>(),
            1.5,
            81870.8);
    }

    public double Run(PrimaryLeaveRequestClaim item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryLeaveRequestClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
