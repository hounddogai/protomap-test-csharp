namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding notice is in its life cycle.</summary>
public enum OffboardingNoticeStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A offboarding notice as the offboarding team keeps it.</summary>
public sealed class OffboardingNotice(string code, string title, string category, string ownerId, double threshold, double discount, int batchSize, int durationMinutes, bool archived, OffboardingNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string OwnerId { get; set; } = ownerId;
    public double Threshold { get; set; } = threshold;
    public double Discount { get; set; } = discount;
    public int BatchSize { get; set; } = batchSize;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Archived { get; set; } = archived;
    public OffboardingNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding notice in status planned.</summary>
    public static OffboardingNotice NewOffboardingNotice(string code, string title)
    {
        return new OffboardingNotice(code, title, "", "", 0.0, 0.0, 0, 0, false, OffboardingNoticeStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingNoticeStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingNoticeStatus next)
    {
        switch (Status)
        {
            case OffboardingNoticeStatus.Planned:
                return next == OffboardingNoticeStatus.InProgress || next == OffboardingNoticeStatus.Cancelled;
            case OffboardingNoticeStatus.Scheduled:
                switch (next)
                {
                    case OffboardingNoticeStatus.Completed:
                        return true;
                }
                return false;
            case OffboardingNoticeStatus.InProgress:
                switch (next)
                {
                    case OffboardingNoticeStatus.Completed:
                        return true;
                }
                return false;
            case OffboardingNoticeStatus.Completed:
                switch (next)
                {
                    case OffboardingNoticeStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)DurationMinutes * Threshold, 2);
    }

    public OffboardingNotice Copy()
    {
        return new OffboardingNotice(
            Code,
            Title,
            Category,
            OwnerId,
            Threshold,
            Discount,
            BatchSize,
            DurationMinutes,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
