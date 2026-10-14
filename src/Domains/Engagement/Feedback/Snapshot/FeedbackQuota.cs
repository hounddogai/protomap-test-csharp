namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback quota is in its life cycle.</summary>
public enum FeedbackQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A feedback quota as the feedback team keeps it.</summary>
public sealed class FeedbackQuota(string code, string name, string region, string salaryBand, int durationMinutes, bool archived, double ratio, List<string> tags, FeedbackQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public string SalaryBand { get; set; } = salaryBand;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Archived { get; set; } = archived;
    public double Ratio { get; set; } = ratio;
    public List<string> Tags { get; set; } = tags;
    public FeedbackQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a feedback quota in status created.</summary>
    public static FeedbackQuota NewFeedbackQuota(string code, string name)
    {
        return new FeedbackQuota(code, name, "", "", 0, false, 0.0, new List<string>(), FeedbackQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackQuotaStatus next)
    {
        switch (Status)
        {
            case FeedbackQuotaStatus.Created:
                switch (next)
                {
                    case FeedbackQuotaStatus.Published:
                        return true;
                }
                return false;
            case FeedbackQuotaStatus.Validated:
                return next == FeedbackQuotaStatus.Published || next == FeedbackQuotaStatus.Retired;
            case FeedbackQuotaStatus.Published:
                return next == FeedbackQuotaStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FeedbackQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double WeightedExposure()
    {
        return Math.Round((double)DurationMinutes - Ratio, 2);
    }

    public FeedbackQuota Copy()
    {
        return new FeedbackQuota(Code, Name, Region, SalaryBand, DurationMinutes, Archived, Ratio, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
