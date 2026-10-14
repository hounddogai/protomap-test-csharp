namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request draft is in its life cycle.</summary>
public enum LeaveRequestDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A leave request draft as the leave request team keeps it.</summary>
public sealed class LeaveRequestDraft(string code, string title, int endsAt, bool archived, string channel, string currency, double rate, bool active, string country, string location, LeaveRequestDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public bool Archived { get; set; } = archived;
    public string Channel { get; set; } = channel;
    public string Currency { get; set; } = currency;
    public double Rate { get; set; } = rate;
    public bool Active { get; set; } = active;
    public string Country { get; set; } = country;
    public string Location { get; set; } = location;
    public LeaveRequestDraftStatus Status { get; set; } = status;

    /// <summary>Starts a leave request draft in status planned.</summary>
    public static LeaveRequestDraft NewLeaveRequestDraft(string code, string title)
    {
        return new LeaveRequestDraft(
            code,
            title,
            0,
            false,
            "",
            "",
            0.0,
            false,
            "",
            "",
            LeaveRequestDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestDraftStatus next)
    {
        switch (Status)
        {
            case LeaveRequestDraftStatus.Planned:
                switch (next)
                {
                    case LeaveRequestDraftStatus.InProgress:
                    case LeaveRequestDraftStatus.Completed:
                        return true;
                }
                return false;
            case LeaveRequestDraftStatus.Scheduled:
                return next == LeaveRequestDraftStatus.InProgress;
            case LeaveRequestDraftStatus.InProgress:
                return next == LeaveRequestDraftStatus.Completed || next == LeaveRequestDraftStatus.Cancelled;
            case LeaveRequestDraftStatus.Completed:
                return next == LeaveRequestDraftStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LeaveRequestDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"leave request draft"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)EndsAt - Rate, 2);
    }

    public LeaveRequestDraft Copy()
    {
        return new LeaveRequestDraft(
            Code,
            Title,
            EndsAt,
            Archived,
            Channel,
            Currency,
            Rate,
            Active,
            Country,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
