namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy equity grant segment is in its life cycle.</summary>
public enum LegacyEquityGrantSegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A legacy equity grant segment. Its code is unique within a tenant.</summary>
public sealed class LegacyEquityGrantSegment(string code, string title, int createdAt, int sequence, bool approved, bool pinned, string salaryBand, double amount, LegacyEquityGrantSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public int Sequence { get; set; } = sequence;
    public bool Approved { get; set; } = approved;
    public bool Pinned { get; set; } = pinned;
    public string SalaryBand { get; set; } = salaryBand;
    public double Amount { get; set; } = amount;
    public LegacyEquityGrantSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a legacy equity grant segment in status idle.</summary>
    public static LegacyEquityGrantSegment NewLegacyEquityGrantSegment(string code, string title)
    {
        return new LegacyEquityGrantSegment(
            code,
            title,
            0,
            0,
            false,
            false,
            "",
            0.0,
            LegacyEquityGrantSegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyEquityGrantSegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyEquityGrantSegmentStatus next)
    {
        switch (Status)
        {
            case LegacyEquityGrantSegmentStatus.Idle:
                return next == LegacyEquityGrantSegmentStatus.Ready;
            case LegacyEquityGrantSegmentStatus.Warming:
                switch (next)
                {
                    case LegacyEquityGrantSegmentStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyEquityGrantSegmentStatus.Ready:
                switch (next)
                {
                    case LegacyEquityGrantSegmentStatus.Draining:
                        return true;
                }
                return false;
            case LegacyEquityGrantSegmentStatus.Draining:
                switch (next)
                {
                    case LegacyEquityGrantSegmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyEquityGrantSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy equity grant segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Sequence - Amount, 2);
    }

    public LegacyEquityGrantSegment Copy()
    {
        return new LegacyEquityGrantSegment(
            Code,
            Title,
            CreatedAt,
            Sequence,
            Approved,
            Pinned,
            SalaryBand,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
