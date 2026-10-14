namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive engagement notice is in its life cycle.</summary>
public enum AdaptiveEngagementNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A adaptive engagement notice as the engagement team keeps it.</summary>
public sealed class AdaptiveEngagementNotice(string code, string title, double minimum, string timezone, int limit, int graceDays, int expiresAt, string salaryBand, int attempts, bool active, double threshold, AdaptiveEngagementNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string Timezone { get; set; } = timezone;
    public int Limit { get; set; } = limit;
    public int GraceDays { get; set; } = graceDays;
    public int ExpiresAt { get; set; } = expiresAt;
    public string SalaryBand { get; set; } = salaryBand;
    public int Attempts { get; set; } = attempts;
    public bool Active { get; set; } = active;
    public double Threshold { get; set; } = threshold;
    public AdaptiveEngagementNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive engagement notice in status new.</summary>
    public static AdaptiveEngagementNotice NewAdaptiveEngagementNotice(string code, string title)
    {
        return new AdaptiveEngagementNotice(
            code,
            title,
            0.0,
            "",
            0,
            0,
            0,
            "",
            0,
            false,
            0.0,
            AdaptiveEngagementNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveEngagementNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveEngagementNoticeStatus next)
    {
        switch (Status)
        {
            case AdaptiveEngagementNoticeStatus.New:
                return next == AdaptiveEngagementNoticeStatus.Done || next == AdaptiveEngagementNoticeStatus.Failed;
            case AdaptiveEngagementNoticeStatus.Queued:
                switch (next)
                {
                    case AdaptiveEngagementNoticeStatus.Running:
                    case AdaptiveEngagementNoticeStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveEngagementNoticeStatus.Running:
                switch (next)
                {
                    case AdaptiveEngagementNoticeStatus.Done:
                        return true;
                }
                return false;
            case AdaptiveEngagementNoticeStatus.Done:
                return next == AdaptiveEngagementNoticeStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdaptiveEngagementNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive engagement notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Threshold + (double)Limit, 2);
    }

    public AdaptiveEngagementNotice Copy()
    {
        return new AdaptiveEngagementNotice(
            Code,
            Title,
            Minimum,
            Timezone,
            Limit,
            GraceDays,
            ExpiresAt,
            SalaryBand,
            Attempts,
            Active,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
