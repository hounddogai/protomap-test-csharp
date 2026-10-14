namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched skill profile audit is in its life cycle.</summary>
public enum BatchedSkillProfileAuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One batched skill profile audit, with what the planners need to act on it.</summary>
public sealed class BatchedSkillProfileAudit(string code, string name, int endsAt, double budget, double floorValue, BatchedSkillProfileAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public double Budget { get; set; } = budget;
    public double FloorValue { get; set; } = floorValue;
    public BatchedSkillProfileAuditStatus Status { get; set; } = status;

    /// <summary>Starts a batched skill profile audit in status planned.</summary>
    public static BatchedSkillProfileAudit NewBatchedSkillProfileAudit(string code, string name)
    {
        return new BatchedSkillProfileAudit(code, name, 0, 0.0, 0.0, BatchedSkillProfileAuditStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedSkillProfileAuditStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedSkillProfileAuditStatus next)
    {
        switch (Status)
        {
            case BatchedSkillProfileAuditStatus.Planned:
                return next == BatchedSkillProfileAuditStatus.InProgress || next == BatchedSkillProfileAuditStatus.Completed;
            case BatchedSkillProfileAuditStatus.Scheduled:
                return next == BatchedSkillProfileAuditStatus.InProgress || next == BatchedSkillProfileAuditStatus.Cancelled;
            case BatchedSkillProfileAuditStatus.InProgress:
                return next == BatchedSkillProfileAuditStatus.Cancelled;
            case BatchedSkillProfileAuditStatus.Completed:
                return next == BatchedSkillProfileAuditStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BatchedSkillProfileAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched skill profile audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)EndsAt * FloorValue, 2);
    }

    public BatchedSkillProfileAudit Copy()
    {
        return new BatchedSkillProfileAudit(Code, Name, EndsAt, Budget, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
