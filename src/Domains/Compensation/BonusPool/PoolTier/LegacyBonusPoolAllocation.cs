namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy bonus pool allocation is in its life cycle.</summary>
public enum LegacyBonusPoolAllocationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A legacy bonus pool allocation. Its code is unique within a tenant.</summary>
public sealed class LegacyBonusPoolAllocation(string code, string title, int leaveDays, double amount, string segment, LegacyBonusPoolAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public double Amount { get; set; } = amount;
    public string Segment { get; set; } = segment;
    public LegacyBonusPoolAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a legacy bonus pool allocation in status new.</summary>
    public static LegacyBonusPoolAllocation NewLegacyBonusPoolAllocation(string code, string title)
    {
        return new LegacyBonusPoolAllocation(code, title, 0, 0.0, "", LegacyBonusPoolAllocationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyBonusPoolAllocationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyBonusPoolAllocationStatus next)
    {
        switch (Status)
        {
            case LegacyBonusPoolAllocationStatus.New:
                switch (next)
                {
                    case LegacyBonusPoolAllocationStatus.Queued:
                    case LegacyBonusPoolAllocationStatus.Running:
                        return true;
                }
                return false;
            case LegacyBonusPoolAllocationStatus.Queued:
                switch (next)
                {
                    case LegacyBonusPoolAllocationStatus.Done:
                        return true;
                }
                return false;
            case LegacyBonusPoolAllocationStatus.Running:
                switch (next)
                {
                    case LegacyBonusPoolAllocationStatus.Done:
                    case LegacyBonusPoolAllocationStatus.Failed:
                        return true;
                }
                return false;
            case LegacyBonusPoolAllocationStatus.Done:
                return next == LegacyBonusPoolAllocationStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LegacyBonusPoolAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy bonus pool allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Amount * (double)LeaveDays, 2);
    }

    public LegacyBonusPoolAllocation Copy()
    {
        return new LegacyBonusPoolAllocation(Code, Title, LeaveDays, Amount, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
