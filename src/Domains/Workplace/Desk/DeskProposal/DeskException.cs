namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk exception is in its life cycle.</summary>
public enum DeskExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a desk exception and its current status.</summary>
public sealed class DeskException(string code, string title, int leaveDays, bool pinned, string ownerId, DeskExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Pinned { get; set; } = pinned;
    public string OwnerId { get; set; } = ownerId;
    public DeskExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a desk exception in status proposed.</summary>
    public static DeskException NewDeskException(string code, string title)
    {
        return new DeskException(code, title, 0, false, "", DeskExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskExceptionStatus next)
    {
        switch (Status)
        {
            case DeskExceptionStatus.Proposed:
                switch (next)
                {
                    case DeskExceptionStatus.Approved:
                    case DeskExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
            case DeskExceptionStatus.Approved:
                return next == DeskExceptionStatus.Rejected || next == DeskExceptionStatus.Withdrawn;
            case DeskExceptionStatus.Rejected:
                return next == DeskExceptionStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DeskExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public DeskException Copy()
    {
        return new DeskException(Code, Title, LeaveDays, Pinned, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
