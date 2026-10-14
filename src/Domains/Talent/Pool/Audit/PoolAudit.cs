namespace Acme.Workforce.Talent.Pool.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool audit is in its life cycle.</summary>
public enum PoolAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One pool audit, with what the planners need to act on it.</summary>
public sealed class PoolAudit(string code, string title, int expiresAt, string segment, string employeeId, bool visible, string skill, int retries, double cost, PoolAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Segment { get; set; } = segment;
    public string EmployeeId { get; set; } = employeeId;
    public bool Visible { get; set; } = visible;
    public string Skill { get; set; } = skill;
    public int Retries { get; set; } = retries;
    public double Cost { get; set; } = cost;
    public PoolAuditStatus Status { get; set; } = status;

    /// <summary>Starts a pool audit in status draft.</summary>
    public static PoolAudit NewPoolAudit(string code, string title)
    {
        return new PoolAudit(code, title, 0, "", "", false, "", 0, 0.0, PoolAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolAuditStatus next)
    {
        switch (Status)
        {
            case PoolAuditStatus.Draft:
                switch (next)
                {
                    case PoolAuditStatus.Pending:
                    case PoolAuditStatus.Active:
                        return true;
                }
                return false;
            case PoolAuditStatus.Pending:
                switch (next)
                {
                    case PoolAuditStatus.Active:
                    case PoolAuditStatus.Suspended:
                        return true;
                }
                return false;
            case PoolAuditStatus.Active:
                switch (next)
                {
                    case PoolAuditStatus.Suspended:
                        return true;
                }
                return false;
            case PoolAuditStatus.Suspended:
                switch (next)
                {
                    case PoolAuditStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Cost - (double)ExpiresAt, 2);
    }

    public PoolAudit Copy()
    {
        return new PoolAudit(Code, Title, ExpiresAt, Segment, EmployeeId, Visible, Skill, Retries, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
