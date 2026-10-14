namespace Acme.Workforce.Assets.Assignment.AssignmentEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending assignment entry is in its life cycle.</summary>
public enum PendingAssignmentEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pending assignment entry as the assignment team keeps it.</summary>
public sealed class PendingAssignmentEntry(string code, string name, int quantity, double minimum, int startsAt, double score, double weight, double threshold, string employeeId, PendingAssignmentEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public double Minimum { get; set; } = minimum;
    public int StartsAt { get; set; } = startsAt;
    public double Score { get; set; } = score;
    public double Weight { get; set; } = weight;
    public double Threshold { get; set; } = threshold;
    public string EmployeeId { get; set; } = employeeId;
    public PendingAssignmentEntryStatus Status { get; set; } = status;

    /// <summary>Starts a pending assignment entry in status new.</summary>
    public static PendingAssignmentEntry NewPendingAssignmentEntry(string code, string name)
    {
        return new PendingAssignmentEntry(
            code,
            name,
            0,
            0.0,
            0,
            0.0,
            0.0,
            0.0,
            "",
            PendingAssignmentEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAssignmentEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAssignmentEntryStatus next)
    {
        switch (Status)
        {
            case PendingAssignmentEntryStatus.New:
                switch (next)
                {
                    case PendingAssignmentEntryStatus.Failed:
                        return true;
                }
                return false;
            case PendingAssignmentEntryStatus.Queued:
                switch (next)
                {
                    case PendingAssignmentEntryStatus.Done:
                        return true;
                }
                return false;
            case PendingAssignmentEntryStatus.Running:
                switch (next)
                {
                    case PendingAssignmentEntryStatus.Failed:
                        return true;
                }
                return false;
            case PendingAssignmentEntryStatus.Done:
                return next == PendingAssignmentEntryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingAssignmentEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending assignment entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Minimum + (double)Quantity, 2);
    }

    public PendingAssignmentEntry Copy()
    {
        return new PendingAssignmentEntry(
            Code,
            Name,
            Quantity,
            Minimum,
            StartsAt,
            Score,
            Weight,
            Threshold,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
