namespace Acme.Workforce.Absence.ReturnPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive return plan entry is in its life cycle.</summary>
public enum AdaptiveReturnPlanEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One adaptive return plan entry, with what the planners need to act on it.</summary>
public sealed class AdaptiveReturnPlanEntry(string code, string name, double threshold, string team, int windowDays, string department, string contactEmail, int limit, int expiresAt, string country, AdaptiveReturnPlanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public string Team { get; set; } = team;
    public int WindowDays { get; set; } = windowDays;
    public string Department { get; set; } = department;
    public string ContactEmail { get; set; } = contactEmail;
    public int Limit { get; set; } = limit;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Country { get; set; } = country;
    public AdaptiveReturnPlanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive return plan entry in status planned.</summary>
    public static AdaptiveReturnPlanEntry NewAdaptiveReturnPlanEntry(string code, string name)
    {
        return new AdaptiveReturnPlanEntry(
            code,
            name,
            0.0,
            "",
            0,
            "",
            "",
            0,
            0,
            "",
            AdaptiveReturnPlanEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveReturnPlanEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveReturnPlanEntryStatus next)
    {
        switch (Status)
        {
            case AdaptiveReturnPlanEntryStatus.Planned:
                return next == AdaptiveReturnPlanEntryStatus.Completed || next == AdaptiveReturnPlanEntryStatus.Cancelled;
            case AdaptiveReturnPlanEntryStatus.Scheduled:
                return next == AdaptiveReturnPlanEntryStatus.InProgress || next == AdaptiveReturnPlanEntryStatus.Completed;
            case AdaptiveReturnPlanEntryStatus.InProgress:
                return next == AdaptiveReturnPlanEntryStatus.Completed || next == AdaptiveReturnPlanEntryStatus.Cancelled;
            case AdaptiveReturnPlanEntryStatus.Completed:
                switch (next)
                {
                    case AdaptiveReturnPlanEntryStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveReturnPlanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive return plan entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"adaptive return plan entry"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)WindowDays + (double)ExpiresAt, 2);
    }

    public AdaptiveReturnPlanEntry Copy()
    {
        return new AdaptiveReturnPlanEntry(
            Code,
            Name,
            Threshold,
            Team,
            WindowDays,
            Department,
            ContactEmail,
            Limit,
            ExpiresAt,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
