namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped access badge draft is in its life cycle.</summary>
public enum CappedAccessBadgeDraftStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One capped access badge draft, with what the planners need to act on it.</summary>
public sealed class CappedAccessBadgeDraft(string code, string label, string employeeId, double ceiling, double fte, string summary, string location, CappedAccessBadgeDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public double Ceiling { get; set; } = ceiling;
    public double Fte { get; set; } = fte;
    public string Summary { get; set; } = summary;
    public string Location { get; set; } = location;
    public CappedAccessBadgeDraftStatus Status { get; set; } = status;

    /// <summary>Starts a capped access badge draft in status draft.</summary>
    public static CappedAccessBadgeDraft NewCappedAccessBadgeDraft(string code, string label)
    {
        return new CappedAccessBadgeDraft(code, label, "", 0.0, 0.0, "", "", CappedAccessBadgeDraftStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAccessBadgeDraftStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAccessBadgeDraftStatus next)
    {
        switch (Status)
        {
            case CappedAccessBadgeDraftStatus.Draft:
                return next == CappedAccessBadgeDraftStatus.Active;
            case CappedAccessBadgeDraftStatus.Pending:
                return next == CappedAccessBadgeDraftStatus.Suspended || next == CappedAccessBadgeDraftStatus.Closed;
            case CappedAccessBadgeDraftStatus.Active:
                return next == CappedAccessBadgeDraftStatus.Closed;
            case CappedAccessBadgeDraftStatus.Suspended:
                return next == CappedAccessBadgeDraftStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CappedAccessBadgeDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped access badge draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Ceiling + Fte, 2);
    }

    public CappedAccessBadgeDraft Copy()
    {
        return new CappedAccessBadgeDraft(Code, Label, EmployeeId, Ceiling, Fte, Summary, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
