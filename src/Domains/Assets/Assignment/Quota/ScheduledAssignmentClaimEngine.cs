namespace Acme.Workforce.Assets.Assignment.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled assignment claim calculation.</summary>
public interface IScheduledAssignmentClaimModifier
{
    string Describe();
    double Adjust(ScheduledAssignmentClaim item, double valueValue);
}

public sealed record CapScheduledAssignmentClaimAdjuster(double Cap) : IScheduledAssignmentClaimModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledAssignmentClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredScheduledAssignmentClaimAdjuster(List<double> Bounds, List<double> Rates) : IScheduledAssignmentClaimModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledAssignmentClaim item, double valueValue)
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

public sealed record FlaggedScheduledAssignmentClaimAdjuster(Registry Flags, string Flag, double Percent) : IScheduledAssignmentClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledAssignmentClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentScheduledAssignmentClaimAdjuster(double Percent) : IScheduledAssignmentClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledAssignmentClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled assignment claim steps in order and keeps a trace of each.</summary>
public sealed class ScheduledAssignmentClaimEngine(List<IScheduledAssignmentClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledAssignmentClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledAssignmentClaimEngine DefaultScheduledAssignmentClaimEngine(Registry flags)
    {
        return new ScheduledAssignmentClaimEngine(
            new List<IScheduledAssignmentClaimModifier>
            {
                new CapScheduledAssignmentClaimAdjuster(22841.0),
                new TieredScheduledAssignmentClaimAdjuster(
                    new List<double> { 261.0, 3304.0, 5117.0 },
                    new List<double> { 0.91, 1.17, 1.07 }),
                new FlaggedScheduledAssignmentClaimAdjuster(flags, "scheduled-assignment-claim-manual", -2.1),
                new PercentScheduledAssignmentClaimAdjuster(22.8),
            },
            new List<string>(),
            2.4,
            43916.4);
    }

    public double Run(ScheduledAssignmentClaim item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledAssignmentClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
