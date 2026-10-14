namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy accrual segment is in its life cycle.</summary>
public enum LegacyAccrualSegmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A legacy accrual segment as the accrual team keeps it.</summary>
public sealed class LegacyAccrualSegment(string code, string title, double score, string costCenter, int updatedAt, string notes, LegacyAccrualSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string CostCenter { get; set; } = costCenter;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Notes { get; set; } = notes;
    public LegacyAccrualSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a legacy accrual segment in status planned.</summary>
    public static LegacyAccrualSegment NewLegacyAccrualSegment(string code, string title)
    {
        return new LegacyAccrualSegment(code, title, 0.0, "", 0, "", LegacyAccrualSegmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyAccrualSegmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyAccrualSegmentStatus next)
    {
        switch (Status)
        {
            case LegacyAccrualSegmentStatus.Planned:
                return next == LegacyAccrualSegmentStatus.Completed;
            case LegacyAccrualSegmentStatus.Scheduled:
                return next == LegacyAccrualSegmentStatus.Cancelled;
            case LegacyAccrualSegmentStatus.InProgress:
                return next == LegacyAccrualSegmentStatus.Completed || next == LegacyAccrualSegmentStatus.Cancelled;
            case LegacyAccrualSegmentStatus.Completed:
                switch (next)
                {
                    case LegacyAccrualSegmentStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyAccrualSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy accrual segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)UpdatedAt - Score, 2);
    }

    public LegacyAccrualSegment Copy()
    {
        return new LegacyAccrualSegment(Code, Title, Score, CostCenter, UpdatedAt, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
