namespace Acme.Workforce.Compensation.Adjustment.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched adjustment tier is in its life cycle.</summary>
public enum BatchedAdjustmentTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A batched adjustment tier as the adjustment team keeps it.</summary>
public sealed class BatchedAdjustmentTier(string code, string title, int tenureMonths, bool archived, string currency, double floorValue, int createdAt, string team, double discount, int leaveDays, double rate, BatchedAdjustmentTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Archived { get; set; } = archived;
    public string Currency { get; set; } = currency;
    public double FloorValue { get; set; } = floorValue;
    public int CreatedAt { get; set; } = createdAt;
    public string Team { get; set; } = team;
    public double Discount { get; set; } = discount;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rate { get; set; } = rate;
    public BatchedAdjustmentTierStatus Status { get; set; } = status;

    /// <summary>Starts a batched adjustment tier in status open.</summary>
    public static BatchedAdjustmentTier NewBatchedAdjustmentTier(string code, string title)
    {
        return new BatchedAdjustmentTier(
            code,
            title,
            0,
            false,
            "",
            0.0,
            0,
            "",
            0.0,
            0,
            0.0,
            BatchedAdjustmentTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAdjustmentTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAdjustmentTierStatus next)
    {
        switch (Status)
        {
            case BatchedAdjustmentTierStatus.Open:
                switch (next)
                {
                    case BatchedAdjustmentTierStatus.Resolved:
                    case BatchedAdjustmentTierStatus.Reopened:
                        return true;
                }
                return false;
            case BatchedAdjustmentTierStatus.InReview:
                return next == BatchedAdjustmentTierStatus.Archived;
            case BatchedAdjustmentTierStatus.Resolved:
                return next == BatchedAdjustmentTierStatus.Reopened || next == BatchedAdjustmentTierStatus.Archived;
            case BatchedAdjustmentTierStatus.Reopened:
                return next == BatchedAdjustmentTierStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BatchedAdjustmentTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched adjustment tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetLoad()
    {
        return Math.Round(Discount * (double)TenureMonths, 2);
    }

    public BatchedAdjustmentTier Copy()
    {
        return new BatchedAdjustmentTier(
            Code,
            Title,
            TenureMonths,
            Archived,
            Currency,
            FloorValue,
            CreatedAt,
            Team,
            Discount,
            LeaveDays,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
