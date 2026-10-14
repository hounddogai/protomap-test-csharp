namespace Acme.Workforce.Assets.Assignment.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment override is in its life cycle.</summary>
public enum AssignmentOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A assignment override. Its code is unique within a tenant.</summary>
public sealed class AssignmentOverride(string code, string label, bool pinned, int quantity, string team, bool approved, string source, double ceiling, string externalRef, AssignmentOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public int Quantity { get; set; } = quantity;
    public string Team { get; set; } = team;
    public bool Approved { get; set; } = approved;
    public string Source { get; set; } = source;
    public double Ceiling { get; set; } = ceiling;
    public string ExternalRef { get; set; } = externalRef;
    public AssignmentOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a assignment override in status planned.</summary>
    public static AssignmentOverride NewAssignmentOverride(string code, string label)
    {
        return new AssignmentOverride(
            code,
            label,
            false,
            0,
            "",
            false,
            "",
            0.0,
            "",
            AssignmentOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentOverrideStatus next)
    {
        switch (Status)
        {
            case AssignmentOverrideStatus.Planned:
                switch (next)
                {
                    case AssignmentOverrideStatus.InProgress:
                    case AssignmentOverrideStatus.Cancelled:
                        return true;
                }
                return false;
            case AssignmentOverrideStatus.Scheduled:
                switch (next)
                {
                    case AssignmentOverrideStatus.InProgress:
                    case AssignmentOverrideStatus.Completed:
                        return true;
                }
                return false;
            case AssignmentOverrideStatus.InProgress:
                return next == AssignmentOverrideStatus.Completed || next == AssignmentOverrideStatus.Cancelled;
            case AssignmentOverrideStatus.Completed:
                return next == AssignmentOverrideStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AssignmentOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Quantity * Ceiling, 2);
    }

    public AssignmentOverride Copy()
    {
        return new AssignmentOverride(
            Code,
            Label,
            Pinned,
            Quantity,
            Team,
            Approved,
            Source,
            Ceiling,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
