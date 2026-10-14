namespace Acme.Workforce.Compliance.Audit.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit summary is in its life cycle.</summary>
public enum AuditSummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One audit summary, with what the planners need to act on it.</summary>
public sealed class AuditSummary(string code, string label, double discount, int expiresAt, string grade, string ownerId, int windowDays, string badge, AuditSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Grade { get; set; } = grade;
    public string OwnerId { get; set; } = ownerId;
    public int WindowDays { get; set; } = windowDays;
    public string Badge { get; set; } = badge;
    public AuditSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a audit summary in status proposed.</summary>
    public static AuditSummary NewAuditSummary(string code, string label)
    {
        return new AuditSummary(code, label, 0.0, 0, "", "", 0, "", AuditSummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditSummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditSummaryStatus next)
    {
        switch (Status)
        {
            case AuditSummaryStatus.Proposed:
                switch (next)
                {
                    case AuditSummaryStatus.Approved:
                    case AuditSummaryStatus.Rejected:
                        return true;
                }
                return false;
            case AuditSummaryStatus.Approved:
                switch (next)
                {
                    case AuditSummaryStatus.Rejected:
                    case AuditSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
            case AuditSummaryStatus.Rejected:
                return next == AuditSummaryStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AuditSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"audit summary"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)ExpiresAt - (double)WindowDays, 2);
    }

    public AuditSummary Copy()
    {
        return new AuditSummary(Code, Label, Discount, ExpiresAt, Grade, OwnerId, WindowDays, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
