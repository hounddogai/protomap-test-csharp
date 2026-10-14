namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict feedback quota is in its life cycle.</summary>
public enum StrictFeedbackQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A strict feedback quota as the feedback team keeps it.</summary>
public sealed class StrictFeedbackQuota(string code, string title, string checksum, string unit, int retries, int capacity, string managerId, string currency, string team, string grade, int windowDays, StrictFeedbackQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public string Unit { get; set; } = unit;
    public int Retries { get; set; } = retries;
    public int Capacity { get; set; } = capacity;
    public string ManagerId { get; set; } = managerId;
    public string Currency { get; set; } = currency;
    public string Team { get; set; } = team;
    public string Grade { get; set; } = grade;
    public int WindowDays { get; set; } = windowDays;
    public StrictFeedbackQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a strict feedback quota in status open.</summary>
    public static StrictFeedbackQuota NewStrictFeedbackQuota(string code, string title)
    {
        return new StrictFeedbackQuota(code, title, "", "", 0, 0, "", "", "", "", 0, StrictFeedbackQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictFeedbackQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictFeedbackQuotaStatus next)
    {
        switch (Status)
        {
            case StrictFeedbackQuotaStatus.Open:
                return next == StrictFeedbackQuotaStatus.Archived;
            case StrictFeedbackQuotaStatus.InReview:
                return next == StrictFeedbackQuotaStatus.Resolved;
            case StrictFeedbackQuotaStatus.Resolved:
                return next == StrictFeedbackQuotaStatus.Reopened || next == StrictFeedbackQuotaStatus.Archived;
            case StrictFeedbackQuotaStatus.Reopened:
                return next == StrictFeedbackQuotaStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictFeedbackQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict feedback quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetTotal()
    {
        return Math.Round((double)WindowDays + (double)Capacity, 2);
    }

    public StrictFeedbackQuota Copy()
    {
        return new StrictFeedbackQuota(
            Code,
            Title,
            Checksum,
            Unit,
            Retries,
            Capacity,
            ManagerId,
            Currency,
            Team,
            Grade,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
