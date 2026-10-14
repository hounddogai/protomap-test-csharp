namespace Acme.Workforce.Payrollops.RetroPay.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay milestone is in its life cycle.</summary>
public enum RetroPayMilestoneStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a retro pay milestone and its current status.</summary>
public sealed class RetroPayMilestone(string code, string label, int startsAt, bool active, string checksum, int windowDays, string notes, int capacity, string employeeId, string title, RetroPayMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public bool Active { get; set; } = active;
    public string Checksum { get; set; } = checksum;
    public int WindowDays { get; set; } = windowDays;
    public string Notes { get; set; } = notes;
    public int Capacity { get; set; } = capacity;
    public string EmployeeId { get; set; } = employeeId;
    public string Title { get; set; } = title;
    public RetroPayMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay milestone in status open.</summary>
    public static RetroPayMilestone NewRetroPayMilestone(string code, string label)
    {
        return new RetroPayMilestone(code, label, 0, false, "", 0, "", 0, "", "", RetroPayMilestoneStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayMilestoneStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayMilestoneStatus next)
    {
        switch (Status)
        {
            case RetroPayMilestoneStatus.Open:
                switch (next)
                {
                    case RetroPayMilestoneStatus.InReview:
                    case RetroPayMilestoneStatus.Resolved:
                        return true;
                }
                return false;
            case RetroPayMilestoneStatus.InReview:
                switch (next)
                {
                    case RetroPayMilestoneStatus.Reopened:
                    case RetroPayMilestoneStatus.Archived:
                        return true;
                }
                return false;
            case RetroPayMilestoneStatus.Resolved:
                switch (next)
                {
                    case RetroPayMilestoneStatus.Reopened:
                    case RetroPayMilestoneStatus.Archived:
                        return true;
                }
                return false;
            case RetroPayMilestoneStatus.Reopened:
                return next == RetroPayMilestoneStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RetroPayMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"retro pay milestone"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)WindowDays * (double)StartsAt, 2);
    }

    public RetroPayMilestone Copy()
    {
        return new RetroPayMilestone(
            Code,
            Label,
            StartsAt,
            Active,
            Checksum,
            WindowDays,
            Notes,
            Capacity,
            EmployeeId,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
