namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment audit is in its life cycle.</summary>
public enum AssignmentAuditStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One assignment audit, with what the planners need to act on it.</summary>
public sealed class AssignmentAudit(string code, string label, string managerId, double rating, string team, string description, string phone, int revision, AssignmentAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public double Rating { get; set; } = rating;
    public string Team { get; set; } = team;
    public string Description { get; set; } = description;
    public string Phone { get; set; } = phone;
    public int Revision { get; set; } = revision;
    public AssignmentAuditStatus Status { get; set; } = status;

    /// <summary>Starts a assignment audit in status open.</summary>
    public static AssignmentAudit NewAssignmentAudit(string code, string label)
    {
        return new AssignmentAudit(code, label, "", 0.0, "", "", "", 0, AssignmentAuditStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentAuditStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentAuditStatus next)
    {
        switch (Status)
        {
            case AssignmentAuditStatus.Open:
                switch (next)
                {
                    case AssignmentAuditStatus.Resolved:
                    case AssignmentAuditStatus.Archived:
                        return true;
                }
                return false;
            case AssignmentAuditStatus.InReview:
                return next == AssignmentAuditStatus.Reopened;
            case AssignmentAuditStatus.Resolved:
                switch (next)
                {
                    case AssignmentAuditStatus.Archived:
                        return true;
                }
                return false;
            case AssignmentAuditStatus.Reopened:
                return next == AssignmentAuditStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AssignmentAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)Revision + Rating, 2);
    }

    public AssignmentAudit Copy()
    {
        return new AssignmentAudit(Code, Label, ManagerId, Rating, Team, Description, Phone, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
