namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global audit summary is in its life cycle.</summary>
public enum GlobalAuditSummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A global audit summary. Its code is unique within a tenant.</summary>
public sealed class GlobalAuditSummary(string code, string label, string jobCode, int leaveDays, double minimum, List<string> tags, int limit, double threshold, double score, GlobalAuditSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public int LeaveDays { get; set; } = leaveDays;
    public double Minimum { get; set; } = minimum;
    public List<string> Tags { get; set; } = tags;
    public int Limit { get; set; } = limit;
    public double Threshold { get; set; } = threshold;
    public double Score { get; set; } = score;
    public GlobalAuditSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a global audit summary in status proposed.</summary>
    public static GlobalAuditSummary NewGlobalAuditSummary(string code, string label)
    {
        return new GlobalAuditSummary(
            code,
            label,
            "",
            0,
            0.0,
            new List<string>(),
            0,
            0.0,
            0.0,
            GlobalAuditSummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAuditSummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAuditSummaryStatus next)
    {
        switch (Status)
        {
            case GlobalAuditSummaryStatus.Proposed:
                switch (next)
                {
                    case GlobalAuditSummaryStatus.Approved:
                        return true;
                }
                return false;
            case GlobalAuditSummaryStatus.Approved:
                return next == GlobalAuditSummaryStatus.Withdrawn;
            case GlobalAuditSummaryStatus.Rejected:
                switch (next)
                {
                    case GlobalAuditSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalAuditSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global audit summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double NetLoad()
    {
        return Math.Round(Minimum - (double)Limit, 2);
    }

    public GlobalAuditSummary Copy()
    {
        return new GlobalAuditSummary(
            Code,
            Label,
            JobCode,
            LeaveDays,
            Minimum,
            Tags,
            Limit,
            Threshold,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
