namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback audit is in its life cycle.</summary>
public enum FeedbackAuditStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A feedback audit as the feedback team keeps it.</summary>
public sealed class FeedbackAudit(string code, string name, string title, string phone, int limit, string salaryBand, string source, double minimum, string region, string costCenter, string team, FeedbackAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public int Limit { get; set; } = limit;
    public string SalaryBand { get; set; } = salaryBand;
    public string Source { get; set; } = source;
    public double Minimum { get; set; } = minimum;
    public string Region { get; set; } = region;
    public string CostCenter { get; set; } = costCenter;
    public string Team { get; set; } = team;
    public FeedbackAuditStatus Status { get; set; } = status;

    /// <summary>Starts a feedback audit in status idle.</summary>
    public static FeedbackAudit NewFeedbackAudit(string code, string name)
    {
        return new FeedbackAudit(code, name, "", "", 0, "", "", 0.0, "", "", "", FeedbackAuditStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackAuditStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackAuditStatus next)
    {
        switch (Status)
        {
            case FeedbackAuditStatus.Idle:
                return next == FeedbackAuditStatus.Stopped;
            case FeedbackAuditStatus.Warming:
                return next == FeedbackAuditStatus.Draining;
            case FeedbackAuditStatus.Ready:
                switch (next)
                {
                    case FeedbackAuditStatus.Draining:
                        return true;
                }
                return false;
            case FeedbackAuditStatus.Draining:
                return next == FeedbackAuditStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FeedbackAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"feedback audit"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Limit * Minimum, 2);
    }

    public FeedbackAudit Copy()
    {
        return new FeedbackAudit(
            Code,
            Name,
            Title,
            Phone,
            Limit,
            SalaryBand,
            Source,
            Minimum,
            Region,
            CostCenter,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
