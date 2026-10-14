namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment audit is in its life cycle.</summary>
public enum SentimentAuditStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A sentiment audit. Its code is unique within a tenant.</summary>
public sealed class SentimentAudit(string code, string label, double discount, string department, List<string> tags, int retries, double weight, double cost, double threshold, SentimentAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public string Department { get; set; } = department;
    public List<string> Tags { get; set; } = tags;
    public int Retries { get; set; } = retries;
    public double Weight { get; set; } = weight;
    public double Cost { get; set; } = cost;
    public double Threshold { get; set; } = threshold;
    public SentimentAuditStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment audit in status proposed.</summary>
    public static SentimentAudit NewSentimentAudit(string code, string label)
    {
        return new SentimentAudit(
            code,
            label,
            0.0,
            "",
            new List<string>(),
            0,
            0.0,
            0.0,
            0.0,
            SentimentAuditStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentAuditStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentAuditStatus next)
    {
        switch (Status)
        {
            case SentimentAuditStatus.Proposed:
                return next == SentimentAuditStatus.Approved;
            case SentimentAuditStatus.Approved:
                return next == SentimentAuditStatus.Rejected || next == SentimentAuditStatus.Withdrawn;
            case SentimentAuditStatus.Rejected:
                switch (next)
                {
                    case SentimentAuditStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double AdjustedValue()
    {
        return Math.Round(Discount * Threshold, 2);
    }

    public SentimentAudit Copy()
    {
        return new SentimentAudit(Code, Label, Discount, Department, Tags, Retries, Weight, Cost, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
