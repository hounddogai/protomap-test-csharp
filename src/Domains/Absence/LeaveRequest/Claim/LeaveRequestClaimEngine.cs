namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request claim calculation.</summary>
public interface ILeaveRequestClaimAdjuster
{
    string Describe();
    double Adjust(LeaveRequestClaim item, double valueValue);
}

public sealed record LeaveRequestClaimExpiresAtOffsetAdjuster(double Weight) : ILeaveRequestClaimAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(LeaveRequestClaim item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record RoundingLeaveRequestClaimAdjuster(double Step) : ILeaveRequestClaimAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLeaveRequestClaimAdjuster(double Percent) : ILeaveRequestClaimAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLeaveRequestClaimAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestClaimAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestClaim item, double valueValue)
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

/// <summary>Runs the leave request claim steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestClaimEngine(List<ILeaveRequestClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestClaimEngine DefaultLeaveRequestClaimEngine()
    {
        return new LeaveRequestClaimEngine(
            new List<ILeaveRequestClaimAdjuster>
            {
                new LeaveRequestClaimExpiresAtOffsetAdjuster(1.22),
                new RoundingLeaveRequestClaimAdjuster(1.0),
                new PercentLeaveRequestClaimAdjuster(-12.3),
                new TieredLeaveRequestClaimAdjuster(
                    new List<double> { 3461.0, 7767.0, 7873.0, 9419.0 },
                    new List<double> { 1.15, 0.96, 0.97, 1.07 }),
            },
            new List<string>(),
            0.3,
            44657.3);
    }

    public double Compute(LeaveRequestClaim item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
