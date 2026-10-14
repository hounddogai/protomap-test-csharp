namespace Acme.Workforce.Absence.Holiday.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday proposal is in its life cycle.</summary>
public enum HolidayProposalStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A holiday proposal as the holiday team keeps it.</summary>
public sealed class HolidayProposal(string code, string title, int sequence, int attempts, int startsAt, int windowDays, string managerId, int leaveDays, HolidayProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public int Attempts { get; set; } = attempts;
    public int StartsAt { get; set; } = startsAt;
    public int WindowDays { get; set; } = windowDays;
    public string ManagerId { get; set; } = managerId;
    public int LeaveDays { get; set; } = leaveDays;
    public HolidayProposalStatus Status { get; set; } = status;

    /// <summary>Starts a holiday proposal in status open.</summary>
    public static HolidayProposal NewHolidayProposal(string code, string title)
    {
        return new HolidayProposal(code, title, 0, 0, 0, 0, "", 0, HolidayProposalStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayProposalStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayProposalStatus next)
    {
        switch (Status)
        {
            case HolidayProposalStatus.Open:
                return next == HolidayProposalStatus.Resolved;
            case HolidayProposalStatus.InReview:
                switch (next)
                {
                    case HolidayProposalStatus.Resolved:
                    case HolidayProposalStatus.Reopened:
                        return true;
                }
                return false;
            case HolidayProposalStatus.Resolved:
                return next == HolidayProposalStatus.Reopened || next == HolidayProposalStatus.Archived;
            case HolidayProposalStatus.Reopened:
                switch (next)
                {
                    case HolidayProposalStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"holiday proposal"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)WindowDays + (double)LeaveDays, 2);
    }

    public HolidayProposal Copy()
    {
        return new HolidayProposal(
            Code,
            Title,
            Sequence,
            Attempts,
            StartsAt,
            WindowDays,
            ManagerId,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
