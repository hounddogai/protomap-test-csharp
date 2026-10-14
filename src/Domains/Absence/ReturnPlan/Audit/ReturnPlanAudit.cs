namespace Acme.Workforce.Absence.ReturnPlan.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan audit is in its life cycle.</summary>
public enum ReturnPlanAuditStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One return plan audit, with what the planners need to act on it.</summary>
public sealed class ReturnPlanAudit(string code, string name, string location, int tenureMonths, string ownerId, double fte, bool archived, int createdAt, int batchSize, string title, string grade, ReturnPlanAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public int TenureMonths { get; set; } = tenureMonths;
    public string OwnerId { get; set; } = ownerId;
    public double Fte { get; set; } = fte;
    public bool Archived { get; set; } = archived;
    public int CreatedAt { get; set; } = createdAt;
    public int BatchSize { get; set; } = batchSize;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public ReturnPlanAuditStatus Status { get; set; } = status;

    /// <summary>Starts a return plan audit in status requested.</summary>
    public static ReturnPlanAudit NewReturnPlanAudit(string code, string name)
    {
        return new ReturnPlanAudit(code, name, "", 0, "", 0.0, false, 0, 0, "", "", ReturnPlanAuditStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanAuditStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanAuditStatus next)
    {
        switch (Status)
        {
            case ReturnPlanAuditStatus.Requested:
                switch (next)
                {
                    case ReturnPlanAuditStatus.Revoked:
                    case ReturnPlanAuditStatus.Expired:
                        return true;
                }
                return false;
            case ReturnPlanAuditStatus.Granted:
                switch (next)
                {
                    case ReturnPlanAuditStatus.Revoked:
                    case ReturnPlanAuditStatus.Expired:
                        return true;
                }
                return false;
            case ReturnPlanAuditStatus.Revoked:
                return next == ReturnPlanAuditStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ReturnPlanAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)CreatedAt - Fte, 2);
    }

    public ReturnPlanAudit Copy()
    {
        return new ReturnPlanAudit(
            Code,
            Name,
            Location,
            TenureMonths,
            OwnerId,
            Fte,
            Archived,
            CreatedAt,
            BatchSize,
            Title,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
