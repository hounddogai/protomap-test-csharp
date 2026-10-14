namespace Acme.Workforce.Contractors.Engagement.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement batch is in its life cycle.</summary>
public enum EngagementBatchStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A engagement batch as the engagement team keeps it.</summary>
public sealed class EngagementBatch(string code, string label, bool active, int capacity, double fte, EngagementBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public int Capacity { get; set; } = capacity;
    public double Fte { get; set; } = fte;
    public EngagementBatchStatus Status { get; set; } = status;

    /// <summary>Starts a engagement batch in status proposed.</summary>
    public static EngagementBatch NewEngagementBatch(string code, string label)
    {
        return new EngagementBatch(code, label, false, 0, 0.0, EngagementBatchStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementBatchStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementBatchStatus next)
    {
        switch (Status)
        {
            case EngagementBatchStatus.Proposed:
                return next == EngagementBatchStatus.Approved || next == EngagementBatchStatus.Rejected;
            case EngagementBatchStatus.Approved:
                switch (next)
                {
                    case EngagementBatchStatus.Rejected:
                        return true;
                }
                return false;
            case EngagementBatchStatus.Rejected:
                switch (next)
                {
                    case EngagementBatchStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Capacity * Fte, 2);
    }

    public EngagementBatch Copy()
    {
        return new EngagementBatch(Code, Label, Active, Capacity, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
