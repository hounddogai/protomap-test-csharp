namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary assignment allocation is in its life cycle.</summary>
public enum PrimaryAssignmentAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A primary assignment allocation as the assignment team keeps it.</summary>
public sealed class PrimaryAssignmentAllocation(string code, string name, string label, int tenureMonths, int capacity, PrimaryAssignmentAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Capacity { get; set; } = capacity;
    public PrimaryAssignmentAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a primary assignment allocation in status requested.</summary>
    public static PrimaryAssignmentAllocation NewPrimaryAssignmentAllocation(string code, string name)
    {
        return new PrimaryAssignmentAllocation(code, name, "", 0, 0, PrimaryAssignmentAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAssignmentAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAssignmentAllocationStatus next)
    {
        switch (Status)
        {
            case PrimaryAssignmentAllocationStatus.Requested:
                switch (next)
                {
                    case PrimaryAssignmentAllocationStatus.Revoked:
                    case PrimaryAssignmentAllocationStatus.Expired:
                        return true;
                }
                return false;
            case PrimaryAssignmentAllocationStatus.Granted:
                return next == PrimaryAssignmentAllocationStatus.Revoked;
            case PrimaryAssignmentAllocationStatus.Revoked:
                switch (next)
                {
                    case PrimaryAssignmentAllocationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryAssignmentAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary assignment allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Capacity + (double)TenureMonths, 2);
    }

    public PrimaryAssignmentAllocation Copy()
    {
        return new PrimaryAssignmentAllocation(Code, Name, Label, TenureMonths, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
