namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted merit cycle claim is in its life cycle.</summary>
public enum WeightedMeritCycleClaimStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One weighted merit cycle claim, with what the planners need to act on it.</summary>
public sealed class WeightedMeritCycleClaim(string code, string title, string checksum, string category, string segment, int priority, string location, double threshold, int attempts, WeightedMeritCycleClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public string Category { get; set; } = category;
    public string Segment { get; set; } = segment;
    public int Priority { get; set; } = priority;
    public string Location { get; set; } = location;
    public double Threshold { get; set; } = threshold;
    public int Attempts { get; set; } = attempts;
    public WeightedMeritCycleClaimStatus Status { get; set; } = status;

    /// <summary>Starts a weighted merit cycle claim in status created.</summary>
    public static WeightedMeritCycleClaim NewWeightedMeritCycleClaim(string code, string title)
    {
        return new WeightedMeritCycleClaim(
            code,
            title,
            "",
            "",
            "",
            0,
            "",
            0.0,
            0,
            WeightedMeritCycleClaimStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedMeritCycleClaimStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedMeritCycleClaimStatus next)
    {
        switch (Status)
        {
            case WeightedMeritCycleClaimStatus.Created:
                switch (next)
                {
                    case WeightedMeritCycleClaimStatus.Published:
                    case WeightedMeritCycleClaimStatus.Retired:
                        return true;
                }
                return false;
            case WeightedMeritCycleClaimStatus.Validated:
                switch (next)
                {
                    case WeightedMeritCycleClaimStatus.Retired:
                        return true;
                }
                return false;
            case WeightedMeritCycleClaimStatus.Published:
                return next == WeightedMeritCycleClaimStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WeightedMeritCycleClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted merit cycle claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted merit cycle claim"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Attempts * Threshold, 2);
    }

    public WeightedMeritCycleClaim Copy()
    {
        return new WeightedMeritCycleClaim(
            Code,
            Title,
            Checksum,
            Category,
            Segment,
            Priority,
            Location,
            Threshold,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
