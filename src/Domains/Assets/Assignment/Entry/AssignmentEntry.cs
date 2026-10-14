namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment entry is in its life cycle.</summary>
public enum AssignmentEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a assignment entry and its current status.</summary>
public sealed class AssignmentEntry(string code, string name, double cost, string notes, bool archived, string unit, AssignmentEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public string Notes { get; set; } = notes;
    public bool Archived { get; set; } = archived;
    public string Unit { get; set; } = unit;
    public AssignmentEntryStatus Status { get; set; } = status;

    /// <summary>Starts a assignment entry in status proposed.</summary>
    public static AssignmentEntry NewAssignmentEntry(string code, string name)
    {
        return new AssignmentEntry(code, name, 0.0, "", false, "", AssignmentEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentEntryStatus next)
    {
        switch (Status)
        {
            case AssignmentEntryStatus.Proposed:
                switch (next)
                {
                    case AssignmentEntryStatus.Approved:
                    case AssignmentEntryStatus.Withdrawn:
                        return true;
                }
                return false;
            case AssignmentEntryStatus.Approved:
                return next == AssignmentEntryStatus.Rejected || next == AssignmentEntryStatus.Withdrawn;
            case AssignmentEntryStatus.Rejected:
                return next == AssignmentEntryStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AssignmentEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public AssignmentEntry Copy()
    {
        return new AssignmentEntry(Code, Name, Cost, Notes, Archived, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
