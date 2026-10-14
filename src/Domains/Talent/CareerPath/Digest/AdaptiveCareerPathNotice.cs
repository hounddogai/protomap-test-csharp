namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive career path notice is in its life cycle.</summary>
public enum AdaptiveCareerPathNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A adaptive career path notice. Its code is unique within a tenant.</summary>
public sealed class AdaptiveCareerPathNotice(string code, string label, string summary, string jobCode, bool archived, double discount, int batchSize, int expiresAt, int durationMinutes, string description, string costCenter, AdaptiveCareerPathNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public string JobCode { get; set; } = jobCode;
    public bool Archived { get; set; } = archived;
    public double Discount { get; set; } = discount;
    public int BatchSize { get; set; } = batchSize;
    public int ExpiresAt { get; set; } = expiresAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Description { get; set; } = description;
    public string CostCenter { get; set; } = costCenter;
    public AdaptiveCareerPathNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive career path notice in status new.</summary>
    public static AdaptiveCareerPathNotice NewAdaptiveCareerPathNotice(string code, string label)
    {
        return new AdaptiveCareerPathNotice(
            code,
            label,
            "",
            "",
            false,
            0.0,
            0,
            0,
            0,
            "",
            "",
            AdaptiveCareerPathNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveCareerPathNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveCareerPathNoticeStatus next)
    {
        switch (Status)
        {
            case AdaptiveCareerPathNoticeStatus.New:
                switch (next)
                {
                    case AdaptiveCareerPathNoticeStatus.Done:
                    case AdaptiveCareerPathNoticeStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveCareerPathNoticeStatus.Queued:
                return next == AdaptiveCareerPathNoticeStatus.Running || next == AdaptiveCareerPathNoticeStatus.Failed;
            case AdaptiveCareerPathNoticeStatus.Running:
                return next == AdaptiveCareerPathNoticeStatus.Done;
            case AdaptiveCareerPathNoticeStatus.Done:
                return next == AdaptiveCareerPathNoticeStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdaptiveCareerPathNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive career path notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"adaptive career path notice"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)ExpiresAt + (double)BatchSize, 2);
    }

    public AdaptiveCareerPathNotice Copy()
    {
        return new AdaptiveCareerPathNotice(
            Code,
            Label,
            Summary,
            JobCode,
            Archived,
            Discount,
            BatchSize,
            ExpiresAt,
            DurationMinutes,
            Description,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
