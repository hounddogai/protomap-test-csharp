namespace Acme.Workforce.Talent.Nomination.NominationWindow;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination slot is in its life cycle.</summary>
public enum NominationSlotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a nomination slot and its current status.</summary>
public sealed class NominationSlot(string code, string title, double hours, string team, double minimum, double fte, int leaveDays, string label, NominationSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string Team { get; set; } = team;
    public double Minimum { get; set; } = minimum;
    public double Fte { get; set; } = fte;
    public int LeaveDays { get; set; } = leaveDays;
    public string Label { get; set; } = label;
    public NominationSlotStatus Status { get; set; } = status;

    /// <summary>Starts a nomination slot in status open.</summary>
    public static NominationSlot NewNominationSlot(string code, string title)
    {
        return new NominationSlot(code, title, 0.0, "", 0.0, 0.0, 0, "", NominationSlotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationSlotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationSlotStatus next)
    {
        switch (Status)
        {
            case NominationSlotStatus.Open:
                switch (next)
                {
                    case NominationSlotStatus.InReview:
                        return true;
                }
                return false;
            case NominationSlotStatus.InReview:
                switch (next)
                {
                    case NominationSlotStatus.Resolved:
                    case NominationSlotStatus.Reopened:
                        return true;
                }
                return false;
            case NominationSlotStatus.Resolved:
                return next == NominationSlotStatus.Reopened || next == NominationSlotStatus.Archived;
            case NominationSlotStatus.Reopened:
                return next == NominationSlotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(NominationSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Minimum + Fte, 2);
    }

    public NominationSlot Copy()
    {
        return new NominationSlot(Code, Title, Hours, Team, Minimum, Fte, LeaveDays, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
