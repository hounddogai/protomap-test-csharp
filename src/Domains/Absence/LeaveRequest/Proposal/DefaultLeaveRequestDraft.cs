namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default leave request draft is in its life cycle.</summary>
public enum DefaultLeaveRequestDraftStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One default leave request draft, with what the planners need to act on it.</summary>
public sealed class DefaultLeaveRequestDraft(string code, string label, string segment, int tenureMonths, int durationMinutes, string department, DefaultLeaveRequestDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public int TenureMonths { get; set; } = tenureMonths;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Department { get; set; } = department;
    public DefaultLeaveRequestDraftStatus Status { get; set; } = status;

    /// <summary>Starts a default leave request draft in status created.</summary>
    public static DefaultLeaveRequestDraft NewDefaultLeaveRequestDraft(string code, string label)
    {
        return new DefaultLeaveRequestDraft(code, label, "", 0, 0, "", DefaultLeaveRequestDraftStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultLeaveRequestDraftStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultLeaveRequestDraftStatus next)
    {
        switch (Status)
        {
            case DefaultLeaveRequestDraftStatus.Created:
                switch (next)
                {
                    case DefaultLeaveRequestDraftStatus.Published:
                        return true;
                }
                return false;
            case DefaultLeaveRequestDraftStatus.Validated:
                switch (next)
                {
                    case DefaultLeaveRequestDraftStatus.Published:
                    case DefaultLeaveRequestDraftStatus.Retired:
                        return true;
                }
                return false;
            case DefaultLeaveRequestDraftStatus.Published:
                return next == DefaultLeaveRequestDraftStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DefaultLeaveRequestDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default leave request draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)TenureMonths * (double)DurationMinutes, 2);
    }

    public DefaultLeaveRequestDraft Copy()
    {
        return new DefaultLeaveRequestDraft(Code, Label, Segment, TenureMonths, DurationMinutes, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
