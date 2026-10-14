namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending visitor assignment is in its life cycle.</summary>
public enum PendingVisitorAssignmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a pending visitor assignment and its current status.</summary>
public sealed class PendingVisitorAssignment(string code, string label, int sequence, string name, string region, string managerId, PendingVisitorAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public string ManagerId { get; set; } = managerId;
    public PendingVisitorAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a pending visitor assignment in status requested.</summary>
    public static PendingVisitorAssignment NewPendingVisitorAssignment(string code, string label)
    {
        return new PendingVisitorAssignment(code, label, 0, "", "", "", PendingVisitorAssignmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingVisitorAssignmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingVisitorAssignmentStatus next)
    {
        switch (Status)
        {
            case PendingVisitorAssignmentStatus.Requested:
                return next == PendingVisitorAssignmentStatus.Revoked;
            case PendingVisitorAssignmentStatus.Granted:
                switch (next)
                {
                    case PendingVisitorAssignmentStatus.Revoked:
                    case PendingVisitorAssignmentStatus.Expired:
                        return true;
                }
                return false;
            case PendingVisitorAssignmentStatus.Revoked:
                switch (next)
                {
                    case PendingVisitorAssignmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingVisitorAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending visitor assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public PendingVisitorAssignment Copy()
    {
        return new PendingVisitorAssignment(Code, Label, Sequence, Name, Region, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
