namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched skill profile notice is in its life cycle.</summary>
public enum BatchedSkillProfileNoticeStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A batched skill profile notice as the skill profile team keeps it.</summary>
public sealed class BatchedSkillProfileNotice(string code, string name, double floorValue, string skill, string label, string country, double margin, bool pinned, BatchedSkillProfileNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Skill { get; set; } = skill;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public double Margin { get; set; } = margin;
    public bool Pinned { get; set; } = pinned;
    public BatchedSkillProfileNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a batched skill profile notice in status planned.</summary>
    public static BatchedSkillProfileNotice NewBatchedSkillProfileNotice(string code, string name)
    {
        return new BatchedSkillProfileNotice(
            code,
            name,
            0.0,
            "",
            "",
            "",
            0.0,
            false,
            BatchedSkillProfileNoticeStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedSkillProfileNoticeStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedSkillProfileNoticeStatus next)
    {
        switch (Status)
        {
            case BatchedSkillProfileNoticeStatus.Planned:
                switch (next)
                {
                    case BatchedSkillProfileNoticeStatus.Scheduled:
                    case BatchedSkillProfileNoticeStatus.InProgress:
                        return true;
                }
                return false;
            case BatchedSkillProfileNoticeStatus.Scheduled:
                return next == BatchedSkillProfileNoticeStatus.InProgress || next == BatchedSkillProfileNoticeStatus.Cancelled;
            case BatchedSkillProfileNoticeStatus.InProgress:
                switch (next)
                {
                    case BatchedSkillProfileNoticeStatus.Cancelled:
                        return true;
                }
                return false;
            case BatchedSkillProfileNoticeStatus.Completed:
                switch (next)
                {
                    case BatchedSkillProfileNoticeStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedSkillProfileNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched skill profile notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"batched skill profile notice"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(FloorValue * Margin, 2);
    }

    public BatchedSkillProfileNotice Copy()
    {
        return new BatchedSkillProfileNotice(Code, Name, FloorValue, Skill, Label, Country, Margin, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
