namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient bonus pool digest is in its life cycle.</summary>
public enum LenientBonusPoolDigestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A lenient bonus pool digest as the bonus pool team keeps it.</summary>
public sealed class LenientBonusPoolDigest(string code, string title, int durationMinutes, string segment, List<string> tags, int tenureMonths, LenientBonusPoolDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Segment { get; set; } = segment;
    public List<string> Tags { get; set; } = tags;
    public int TenureMonths { get; set; } = tenureMonths;
    public LenientBonusPoolDigestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient bonus pool digest in status planned.</summary>
    public static LenientBonusPoolDigest NewLenientBonusPoolDigest(string code, string title)
    {
        return new LenientBonusPoolDigest(
            code,
            title,
            0,
            "",
            new List<string>(),
            0,
            LenientBonusPoolDigestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBonusPoolDigestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBonusPoolDigestStatus next)
    {
        switch (Status)
        {
            case LenientBonusPoolDigestStatus.Planned:
                return next == LenientBonusPoolDigestStatus.Cancelled;
            case LenientBonusPoolDigestStatus.Scheduled:
                return next == LenientBonusPoolDigestStatus.Completed;
            case LenientBonusPoolDigestStatus.InProgress:
                switch (next)
                {
                    case LenientBonusPoolDigestStatus.Completed:
                        return true;
                }
                return false;
            case LenientBonusPoolDigestStatus.Completed:
                return next == LenientBonusPoolDigestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientBonusPoolDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient bonus pool digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetTotal()
    {
        return Math.Round((double)DurationMinutes - (double)TenureMonths, 2);
    }

    public LenientBonusPoolDigest Copy()
    {
        return new LenientBonusPoolDigest(Code, Title, DurationMinutes, Segment, Tags, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
