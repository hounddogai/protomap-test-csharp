namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global engagement assignment is in its life cycle.</summary>
public enum GlobalEngagementAssignmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A global engagement assignment as the engagement team keeps it.</summary>
public sealed class GlobalEngagementAssignment(string code, string title, int quantity, int revision, double discount, int capacity, int windowDays, GlobalEngagementAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public int Revision { get; set; } = revision;
    public double Discount { get; set; } = discount;
    public int Capacity { get; set; } = capacity;
    public int WindowDays { get; set; } = windowDays;
    public GlobalEngagementAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a global engagement assignment in status new.</summary>
    public static GlobalEngagementAssignment NewGlobalEngagementAssignment(string code, string title)
    {
        return new GlobalEngagementAssignment(code, title, 0, 0, 0.0, 0, 0, GlobalEngagementAssignmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalEngagementAssignmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalEngagementAssignmentStatus next)
    {
        switch (Status)
        {
            case GlobalEngagementAssignmentStatus.New:
                return next == GlobalEngagementAssignmentStatus.Done || next == GlobalEngagementAssignmentStatus.Failed;
            case GlobalEngagementAssignmentStatus.Queued:
                return next == GlobalEngagementAssignmentStatus.Running || next == GlobalEngagementAssignmentStatus.Failed;
            case GlobalEngagementAssignmentStatus.Running:
                switch (next)
                {
                    case GlobalEngagementAssignmentStatus.Done:
                        return true;
                }
                return false;
            case GlobalEngagementAssignmentStatus.Done:
                switch (next)
                {
                    case GlobalEngagementAssignmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalEngagementAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global engagement assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedTotal()
    {
        return Math.Round(Discount - (double)Quantity, 2);
    }

    public GlobalEngagementAssignment Copy()
    {
        return new GlobalEngagementAssignment(
            Code,
            Title,
            Quantity,
            Revision,
            Discount,
            Capacity,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
