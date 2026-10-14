namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run audit is in its life cycle.</summary>
public enum PayRunAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pay run audit as the pay run team keeps it.</summary>
public sealed class PayRunAudit(string code, string title, double score, double threshold, string managerId, string externalRef, int startsAt, PayRunAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public double Threshold { get; set; } = threshold;
    public string ManagerId { get; set; } = managerId;
    public string ExternalRef { get; set; } = externalRef;
    public int StartsAt { get; set; } = startsAt;
    public PayRunAuditStatus Status { get; set; } = status;

    /// <summary>Starts a pay run audit in status draft.</summary>
    public static PayRunAudit NewPayRunAudit(string code, string title)
    {
        return new PayRunAudit(code, title, 0.0, 0.0, "", "", 0, PayRunAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunAuditStatus next)
    {
        switch (Status)
        {
            case PayRunAuditStatus.Draft:
                return next == PayRunAuditStatus.Closed;
            case PayRunAuditStatus.Pending:
                switch (next)
                {
                    case PayRunAuditStatus.Suspended:
                        return true;
                }
                return false;
            case PayRunAuditStatus.Active:
                switch (next)
                {
                    case PayRunAuditStatus.Suspended:
                    case PayRunAuditStatus.Closed:
                        return true;
                }
                return false;
            case PayRunAuditStatus.Suspended:
                switch (next)
                {
                    case PayRunAuditStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pay run audit"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Threshold * (double)StartsAt, 2);
    }

    public PayRunAudit Copy()
    {
        return new PayRunAudit(Code, Title, Score, Threshold, ManagerId, ExternalRef, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
