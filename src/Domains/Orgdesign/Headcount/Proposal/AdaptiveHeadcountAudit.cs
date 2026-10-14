namespace Acme.Workforce.Orgdesign.Headcount.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive headcount audit is in its life cycle.</summary>
public enum AdaptiveHeadcountAuditStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One adaptive headcount audit, with what the planners need to act on it.</summary>
public sealed class AdaptiveHeadcountAudit(string code, string label, int batchSize, int updatedAt, double threshold, int expiresAt, string grade, string notes, AdaptiveHeadcountAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Threshold { get; set; } = threshold;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Grade { get; set; } = grade;
    public string Notes { get; set; } = notes;
    public AdaptiveHeadcountAuditStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive headcount audit in status open.</summary>
    public static AdaptiveHeadcountAudit NewAdaptiveHeadcountAudit(string code, string label)
    {
        return new AdaptiveHeadcountAudit(code, label, 0, 0, 0.0, 0, "", "", AdaptiveHeadcountAuditStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveHeadcountAuditStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveHeadcountAuditStatus next)
    {
        switch (Status)
        {
            case AdaptiveHeadcountAuditStatus.Open:
                switch (next)
                {
                    case AdaptiveHeadcountAuditStatus.Reopened:
                        return true;
                }
                return false;
            case AdaptiveHeadcountAuditStatus.InReview:
                return next == AdaptiveHeadcountAuditStatus.Reopened || next == AdaptiveHeadcountAuditStatus.Archived;
            case AdaptiveHeadcountAuditStatus.Resolved:
                switch (next)
                {
                    case AdaptiveHeadcountAuditStatus.Reopened:
                    case AdaptiveHeadcountAuditStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveHeadcountAuditStatus.Reopened:
                switch (next)
                {
                    case AdaptiveHeadcountAuditStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveHeadcountAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive headcount audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)UpdatedAt * (double)ExpiresAt, 2);
    }

    public AdaptiveHeadcountAudit Copy()
    {
        return new AdaptiveHeadcountAudit(
            Code,
            Label,
            BatchSize,
            UpdatedAt,
            Threshold,
            ExpiresAt,
            Grade,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
