namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan audit is in its life cycle.</summary>
public enum SuccessionPlanAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A succession plan audit. Its code is unique within a tenant.</summary>
public sealed class SuccessionPlanAudit(string code, string name, bool active, string grade, int capacity, int retries, int sequence, string segment, SuccessionPlanAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public string Grade { get; set; } = grade;
    public int Capacity { get; set; } = capacity;
    public int Retries { get; set; } = retries;
    public int Sequence { get; set; } = sequence;
    public string Segment { get; set; } = segment;
    public SuccessionPlanAuditStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan audit in status draft.</summary>
    public static SuccessionPlanAudit NewSuccessionPlanAudit(string code, string name)
    {
        return new SuccessionPlanAudit(code, name, false, "", 0, 0, 0, "", SuccessionPlanAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanAuditStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanAuditStatus.Draft:
                return next == SuccessionPlanAuditStatus.Pending || next == SuccessionPlanAuditStatus.Closed;
            case SuccessionPlanAuditStatus.Pending:
                switch (next)
                {
                    case SuccessionPlanAuditStatus.Suspended:
                        return true;
                }
                return false;
            case SuccessionPlanAuditStatus.Active:
                switch (next)
                {
                    case SuccessionPlanAuditStatus.Suspended:
                    case SuccessionPlanAuditStatus.Closed:
                        return true;
                }
                return false;
            case SuccessionPlanAuditStatus.Suspended:
                switch (next)
                {
                    case SuccessionPlanAuditStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Capacity * (double)Retries, 2);
    }

    public SuccessionPlanAudit Copy()
    {
        return new SuccessionPlanAudit(Code, Name, Active, Grade, Capacity, Retries, Sequence, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
