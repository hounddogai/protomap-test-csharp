namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict accrual slot is in its life cycle.</summary>
public enum StrictAccrualSlotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A strict accrual slot. Its code is unique within a tenant.</summary>
public sealed class StrictAccrualSlot(string code, string label, double rate, int retries, double ceiling, double threshold, string costCenter, int expiresAt, string department, StrictAccrualSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public int Retries { get; set; } = retries;
    public double Ceiling { get; set; } = ceiling;
    public double Threshold { get; set; } = threshold;
    public string CostCenter { get; set; } = costCenter;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Department { get; set; } = department;
    public StrictAccrualSlotStatus Status { get; set; } = status;

    /// <summary>Starts a strict accrual slot in status open.</summary>
    public static StrictAccrualSlot NewStrictAccrualSlot(string code, string label)
    {
        return new StrictAccrualSlot(code, label, 0.0, 0, 0.0, 0.0, "", 0, "", StrictAccrualSlotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAccrualSlotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAccrualSlotStatus next)
    {
        switch (Status)
        {
            case StrictAccrualSlotStatus.Open:
                return next == StrictAccrualSlotStatus.Archived;
            case StrictAccrualSlotStatus.InReview:
                switch (next)
                {
                    case StrictAccrualSlotStatus.Reopened:
                    case StrictAccrualSlotStatus.Archived:
                        return true;
                }
                return false;
            case StrictAccrualSlotStatus.Resolved:
                return next == StrictAccrualSlotStatus.Archived;
            case StrictAccrualSlotStatus.Reopened:
                return next == StrictAccrualSlotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictAccrualSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict accrual slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)ExpiresAt * Rate, 2);
    }

    public StrictAccrualSlot Copy()
    {
        return new StrictAccrualSlot(
            Code,
            Label,
            Rate,
            Retries,
            Ceiling,
            Threshold,
            CostCenter,
            ExpiresAt,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
