namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped statement of work assignment is in its life cycle.</summary>
public enum CappedStatementOfWorkAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A capped statement of work assignment as the statement of work team keeps it.</summary>
public sealed class CappedStatementOfWorkAssignment(string code, string name, int revision, double ceiling, int version, double cost, string label, string team, CappedStatementOfWorkAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public double Ceiling { get; set; } = ceiling;
    public int Version { get; set; } = version;
    public double Cost { get; set; } = cost;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public CappedStatementOfWorkAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a capped statement of work assignment in status open.</summary>
    public static CappedStatementOfWorkAssignment NewCappedStatementOfWorkAssignment(string code, string name)
    {
        return new CappedStatementOfWorkAssignment(
            code,
            name,
            0,
            0.0,
            0,
            0.0,
            "",
            "",
            CappedStatementOfWorkAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedStatementOfWorkAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedStatementOfWorkAssignmentStatus next)
    {
        switch (Status)
        {
            case CappedStatementOfWorkAssignmentStatus.Open:
                return next == CappedStatementOfWorkAssignmentStatus.InReview;
            case CappedStatementOfWorkAssignmentStatus.InReview:
                switch (next)
                {
                    case CappedStatementOfWorkAssignmentStatus.Resolved:
                    case CappedStatementOfWorkAssignmentStatus.Reopened:
                        return true;
                }
                return false;
            case CappedStatementOfWorkAssignmentStatus.Resolved:
                return next == CappedStatementOfWorkAssignmentStatus.Reopened || next == CappedStatementOfWorkAssignmentStatus.Archived;
            case CappedStatementOfWorkAssignmentStatus.Reopened:
                return next == CappedStatementOfWorkAssignmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(CappedStatementOfWorkAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped statement of work assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ceiling * Cost, 2);
    }

    public CappedStatementOfWorkAssignment Copy()
    {
        return new CappedStatementOfWorkAssignment(
            Code,
            Name,
            Revision,
            Ceiling,
            Version,
            Cost,
            Label,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
