namespace Acme.Workforce.Absence.Balance.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback balance adjustment is in its life cycle.</summary>
public enum FallbackBalanceAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A fallback balance adjustment. Its code is unique within a tenant.</summary>
public sealed class FallbackBalanceAdjustment(string code, string label, bool approved, string grade, double amount, int batchSize, string location, string region, string notes, FallbackBalanceAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public string Grade { get; set; } = grade;
    public double Amount { get; set; } = amount;
    public int BatchSize { get; set; } = batchSize;
    public string Location { get; set; } = location;
    public string Region { get; set; } = region;
    public string Notes { get; set; } = notes;
    public FallbackBalanceAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a fallback balance adjustment in status idle.</summary>
    public static FallbackBalanceAdjustment NewFallbackBalanceAdjustment(string code, string label)
    {
        return new FallbackBalanceAdjustment(
            code,
            label,
            false,
            "",
            0.0,
            0,
            "",
            "",
            "",
            FallbackBalanceAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackBalanceAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackBalanceAdjustmentStatus next)
    {
        switch (Status)
        {
            case FallbackBalanceAdjustmentStatus.Idle:
                switch (next)
                {
                    case FallbackBalanceAdjustmentStatus.Warming:
                    case FallbackBalanceAdjustmentStatus.Draining:
                        return true;
                }
                return false;
            case FallbackBalanceAdjustmentStatus.Warming:
                switch (next)
                {
                    case FallbackBalanceAdjustmentStatus.Ready:
                        return true;
                }
                return false;
            case FallbackBalanceAdjustmentStatus.Ready:
                return next == FallbackBalanceAdjustmentStatus.Draining || next == FallbackBalanceAdjustmentStatus.Stopped;
            case FallbackBalanceAdjustmentStatus.Draining:
                switch (next)
                {
                    case FallbackBalanceAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackBalanceAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback balance adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback balance adjustment"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)BatchSize - Amount, 2);
    }

    public FallbackBalanceAdjustment Copy()
    {
        return new FallbackBalanceAdjustment(
            Code,
            Label,
            Approved,
            Grade,
            Amount,
            BatchSize,
            Location,
            Region,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
