namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped pool audit is in its life cycle.</summary>
public enum CappedPoolAuditStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A capped pool audit. Its code is unique within a tenant.</summary>
public sealed class CappedPoolAudit(string code, string label, int startsAt, string reviewer, int retries, double budget, CappedPoolAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public string Reviewer { get; set; } = reviewer;
    public int Retries { get; set; } = retries;
    public double Budget { get; set; } = budget;
    public CappedPoolAuditStatus Status { get; set; } = status;

    /// <summary>Starts a capped pool audit in status requested.</summary>
    public static CappedPoolAudit NewCappedPoolAudit(string code, string label)
    {
        return new CappedPoolAudit(code, label, 0, "", 0, 0.0, CappedPoolAuditStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPoolAuditStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPoolAuditStatus next)
    {
        switch (Status)
        {
            case CappedPoolAuditStatus.Requested:
                return next == CappedPoolAuditStatus.Revoked || next == CappedPoolAuditStatus.Expired;
            case CappedPoolAuditStatus.Granted:
                switch (next)
                {
                    case CappedPoolAuditStatus.Expired:
                        return true;
                }
                return false;
            case CappedPoolAuditStatus.Revoked:
                switch (next)
                {
                    case CappedPoolAuditStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedPoolAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped pool audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"capped pool audit"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)StartsAt + Budget, 2);
    }

    public CappedPoolAudit Copy()
    {
        return new CappedPoolAudit(Code, Label, StartsAt, Reviewer, Retries, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
