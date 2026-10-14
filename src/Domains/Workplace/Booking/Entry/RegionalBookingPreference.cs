namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional booking preference is in its life cycle.</summary>
public enum RegionalBookingPreferenceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a regional booking preference and its current status.</summary>
public sealed class RegionalBookingPreference(string code, string label, int endsAt, bool approved, string skill, double ceiling, bool active, string summary, string description, int leaveDays, int retries, RegionalBookingPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public bool Approved { get; set; } = approved;
    public string Skill { get; set; } = skill;
    public double Ceiling { get; set; } = ceiling;
    public bool Active { get; set; } = active;
    public string Summary { get; set; } = summary;
    public string Description { get; set; } = description;
    public int LeaveDays { get; set; } = leaveDays;
    public int Retries { get; set; } = retries;
    public RegionalBookingPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a regional booking preference in status planned.</summary>
    public static RegionalBookingPreference NewRegionalBookingPreference(string code, string label)
    {
        return new RegionalBookingPreference(
            code,
            label,
            0,
            false,
            "",
            0.0,
            false,
            "",
            "",
            0,
            0,
            RegionalBookingPreferenceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalBookingPreferenceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalBookingPreferenceStatus next)
    {
        switch (Status)
        {
            case RegionalBookingPreferenceStatus.Planned:
                switch (next)
                {
                    case RegionalBookingPreferenceStatus.Scheduled:
                        return true;
                }
                return false;
            case RegionalBookingPreferenceStatus.Scheduled:
                return next == RegionalBookingPreferenceStatus.Completed;
            case RegionalBookingPreferenceStatus.InProgress:
                return next == RegionalBookingPreferenceStatus.Completed;
            case RegionalBookingPreferenceStatus.Completed:
                return next == RegionalBookingPreferenceStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RegionalBookingPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional booking preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"regional booking preference"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)LeaveDays - (double)Retries, 2);
    }

    public RegionalBookingPreference Copy()
    {
        return new RegionalBookingPreference(
            Code,
            Label,
            EndsAt,
            Approved,
            Skill,
            Ceiling,
            Active,
            Summary,
            Description,
            LeaveDays,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
