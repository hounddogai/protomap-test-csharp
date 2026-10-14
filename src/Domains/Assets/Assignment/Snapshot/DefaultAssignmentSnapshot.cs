namespace Acme.Workforce.Assets.Assignment.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default assignment snapshot is in its life cycle.</summary>
public enum DefaultAssignmentSnapshotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One default assignment snapshot, with what the planners need to act on it.</summary>
public sealed class DefaultAssignmentSnapshot(string code, string label, int attempts, string department, int graceDays, double discount, string contactEmail, string employeeId, double score, string description, DefaultAssignmentSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public string Department { get; set; } = department;
    public int GraceDays { get; set; } = graceDays;
    public double Discount { get; set; } = discount;
    public string ContactEmail { get; set; } = contactEmail;
    public string EmployeeId { get; set; } = employeeId;
    public double Score { get; set; } = score;
    public string Description { get; set; } = description;
    public DefaultAssignmentSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a default assignment snapshot in status draft.</summary>
    public static DefaultAssignmentSnapshot NewDefaultAssignmentSnapshot(string code, string label)
    {
        return new DefaultAssignmentSnapshot(
            code,
            label,
            0,
            "",
            0,
            0.0,
            "",
            "",
            0.0,
            "",
            DefaultAssignmentSnapshotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAssignmentSnapshotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAssignmentSnapshotStatus next)
    {
        switch (Status)
        {
            case DefaultAssignmentSnapshotStatus.Draft:
                switch (next)
                {
                    case DefaultAssignmentSnapshotStatus.Active:
                        return true;
                }
                return false;
            case DefaultAssignmentSnapshotStatus.Pending:
                switch (next)
                {
                    case DefaultAssignmentSnapshotStatus.Suspended:
                        return true;
                }
                return false;
            case DefaultAssignmentSnapshotStatus.Active:
                return next == DefaultAssignmentSnapshotStatus.Suspended || next == DefaultAssignmentSnapshotStatus.Closed;
            case DefaultAssignmentSnapshotStatus.Suspended:
                switch (next)
                {
                    case DefaultAssignmentSnapshotStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultAssignmentSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default assignment snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Attempts * Score, 2);
    }

    public DefaultAssignmentSnapshot Copy()
    {
        return new DefaultAssignmentSnapshot(
            Code,
            Label,
            Attempts,
            Department,
            GraceDays,
            Discount,
            ContactEmail,
            EmployeeId,
            Score,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
