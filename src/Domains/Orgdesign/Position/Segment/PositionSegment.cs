namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position segment is in its life cycle.</summary>
public enum PositionSegmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A position segment as the position team keeps it.</summary>
public sealed class PositionSegment(string code, string label, int expiresAt, double ratio, string source, string checksum, int endsAt, string category, string costCenter, PositionSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Ratio { get; set; } = ratio;
    public string Source { get; set; } = source;
    public string Checksum { get; set; } = checksum;
    public int EndsAt { get; set; } = endsAt;
    public string Category { get; set; } = category;
    public string CostCenter { get; set; } = costCenter;
    public PositionSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a position segment in status planned.</summary>
    public static PositionSegment NewPositionSegment(string code, string label)
    {
        return new PositionSegment(code, label, 0, 0.0, "", "", 0, "", "", PositionSegmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionSegmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionSegmentStatus next)
    {
        switch (Status)
        {
            case PositionSegmentStatus.Planned:
                return next == PositionSegmentStatus.Scheduled || next == PositionSegmentStatus.Cancelled;
            case PositionSegmentStatus.Scheduled:
                switch (next)
                {
                    case PositionSegmentStatus.InProgress:
                        return true;
                }
                return false;
            case PositionSegmentStatus.InProgress:
                return next == PositionSegmentStatus.Completed || next == PositionSegmentStatus.Cancelled;
            case PositionSegmentStatus.Completed:
                return next == PositionSegmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PositionSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Ratio + (double)ExpiresAt, 2);
    }

    public PositionSegment Copy()
    {
        return new PositionSegment(
            Code,
            Label,
            ExpiresAt,
            Ratio,
            Source,
            Checksum,
            EndsAt,
            Category,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
