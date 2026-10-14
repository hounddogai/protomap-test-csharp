namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped visitor assignment is in its life cycle.</summary>
public enum CappedVisitorAssignmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A capped visitor assignment. Its code is unique within a tenant.</summary>
public sealed class CappedVisitorAssignment(string code, string title, string summary, string employeeId, int expiresAt, CappedVisitorAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public string EmployeeId { get; set; } = employeeId;
    public int ExpiresAt { get; set; } = expiresAt;
    public CappedVisitorAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a capped visitor assignment in status new.</summary>
    public static CappedVisitorAssignment NewCappedVisitorAssignment(string code, string title)
    {
        return new CappedVisitorAssignment(code, title, "", "", 0, CappedVisitorAssignmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedVisitorAssignmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedVisitorAssignmentStatus next)
    {
        switch (Status)
        {
            case CappedVisitorAssignmentStatus.New:
                return next == CappedVisitorAssignmentStatus.Failed;
            case CappedVisitorAssignmentStatus.Queued:
                return next == CappedVisitorAssignmentStatus.Done;
            case CappedVisitorAssignmentStatus.Running:
                switch (next)
                {
                    case CappedVisitorAssignmentStatus.Failed:
                        return true;
                }
                return false;
            case CappedVisitorAssignmentStatus.Done:
                return next == CappedVisitorAssignmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CappedVisitorAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped visitor assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public CappedVisitorAssignment Copy()
    {
        return new CappedVisitorAssignment(Code, Title, Summary, EmployeeId, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
