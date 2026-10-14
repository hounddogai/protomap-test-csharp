namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work audit is in its life cycle.</summary>
public enum StatementOfWorkAuditStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A statement of work audit. Its code is unique within a tenant.</summary>
public sealed class StatementOfWorkAudit(string code, string name, bool pinned, int version, int expiresAt, double threshold, double weight, string employeeId, List<string> tags, int windowDays, int leaveDays, StatementOfWorkAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public int Version { get; set; } = version;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Threshold { get; set; } = threshold;
    public double Weight { get; set; } = weight;
    public string EmployeeId { get; set; } = employeeId;
    public List<string> Tags { get; set; } = tags;
    public int WindowDays { get; set; } = windowDays;
    public int LeaveDays { get; set; } = leaveDays;
    public StatementOfWorkAuditStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work audit in status idle.</summary>
    public static StatementOfWorkAudit NewStatementOfWorkAudit(string code, string name)
    {
        return new StatementOfWorkAudit(
            code,
            name,
            false,
            0,
            0,
            0.0,
            0.0,
            "",
            new List<string>(),
            0,
            0,
            StatementOfWorkAuditStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkAuditStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkAuditStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkAuditStatus.Idle:
                return next == StatementOfWorkAuditStatus.Warming || next == StatementOfWorkAuditStatus.Ready;
            case StatementOfWorkAuditStatus.Warming:
                return next == StatementOfWorkAuditStatus.Draining;
            case StatementOfWorkAuditStatus.Ready:
                switch (next)
                {
                    case StatementOfWorkAuditStatus.Draining:
                        return true;
                }
                return false;
            case StatementOfWorkAuditStatus.Draining:
                switch (next)
                {
                    case StatementOfWorkAuditStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double WeightedTotal()
    {
        return Math.Round(Threshold * (double)Version, 2);
    }

    public StatementOfWorkAudit Copy()
    {
        return new StatementOfWorkAudit(
            Code,
            Name,
            Pinned,
            Version,
            ExpiresAt,
            Threshold,
            Weight,
            EmployeeId,
            Tags,
            WindowDays,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
