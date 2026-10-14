namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request profile is in its life cycle.</summary>
public enum LeaveRequestProfileStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a leave request profile and its current status.</summary>
public sealed class LeaveRequestProfile(string code, string label, int windowDays, int startsAt, string location, double rate, int updatedAt, double ceiling, string country, string locale, LeaveRequestProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public int StartsAt { get; set; } = startsAt;
    public string Location { get; set; } = location;
    public double Rate { get; set; } = rate;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Ceiling { get; set; } = ceiling;
    public string Country { get; set; } = country;
    public string Locale { get; set; } = locale;
    public LeaveRequestProfileStatus Status { get; set; } = status;

    /// <summary>Starts a leave request profile in status planned.</summary>
    public static LeaveRequestProfile NewLeaveRequestProfile(string code, string label)
    {
        return new LeaveRequestProfile(
            code,
            label,
            0,
            0,
            "",
            0.0,
            0,
            0.0,
            "",
            "",
            LeaveRequestProfileStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestProfileStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestProfileStatus next)
    {
        switch (Status)
        {
            case LeaveRequestProfileStatus.Planned:
                switch (next)
                {
                    case LeaveRequestProfileStatus.Cancelled:
                        return true;
                }
                return false;
            case LeaveRequestProfileStatus.Scheduled:
                switch (next)
                {
                    case LeaveRequestProfileStatus.Cancelled:
                        return true;
                }
                return false;
            case LeaveRequestProfileStatus.InProgress:
                return next == LeaveRequestProfileStatus.Completed || next == LeaveRequestProfileStatus.Cancelled;
            case LeaveRequestProfileStatus.Completed:
                return next == LeaveRequestProfileStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LeaveRequestProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"leave request profile"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)StartsAt + (double)UpdatedAt, 2);
    }

    public LeaveRequestProfile Copy()
    {
        return new LeaveRequestProfile(
            Code,
            Label,
            WindowDays,
            StartsAt,
            Location,
            Rate,
            UpdatedAt,
            Ceiling,
            Country,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
