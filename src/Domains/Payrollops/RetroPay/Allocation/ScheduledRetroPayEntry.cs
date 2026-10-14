namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled retro pay entry is in its life cycle.</summary>
public enum ScheduledRetroPayEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a scheduled retro pay entry and its current status.</summary>
public sealed class ScheduledRetroPayEntry(string code, string title, string badge, double ratio, string ownerId, int leaveDays, ScheduledRetroPayEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public double Ratio { get; set; } = ratio;
    public string OwnerId { get; set; } = ownerId;
    public int LeaveDays { get; set; } = leaveDays;
    public ScheduledRetroPayEntryStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled retro pay entry in status proposed.</summary>
    public static ScheduledRetroPayEntry NewScheduledRetroPayEntry(string code, string title)
    {
        return new ScheduledRetroPayEntry(code, title, "", 0.0, "", 0, ScheduledRetroPayEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledRetroPayEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledRetroPayEntryStatus next)
    {
        switch (Status)
        {
            case ScheduledRetroPayEntryStatus.Proposed:
                switch (next)
                {
                    case ScheduledRetroPayEntryStatus.Approved:
                        return true;
                }
                return false;
            case ScheduledRetroPayEntryStatus.Approved:
                switch (next)
                {
                    case ScheduledRetroPayEntryStatus.Rejected:
                    case ScheduledRetroPayEntryStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScheduledRetroPayEntryStatus.Rejected:
                switch (next)
                {
                    case ScheduledRetroPayEntryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledRetroPayEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled retro pay entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)LeaveDays * Ratio, 2);
    }

    public ScheduledRetroPayEntry Copy()
    {
        return new ScheduledRetroPayEntry(Code, Title, Badge, Ratio, OwnerId, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
