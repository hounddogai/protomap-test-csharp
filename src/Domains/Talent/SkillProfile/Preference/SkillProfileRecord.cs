namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile record is in its life cycle.</summary>
public enum SkillProfileRecordStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A skill profile record. Its code is unique within a tenant.</summary>
public sealed class SkillProfileRecord(string code, string name, bool pinned, int leaveDays, string location, string channel, string summary, string managerId, double budget, string jobCode, string segment, SkillProfileRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public int LeaveDays { get; set; } = leaveDays;
    public string Location { get; set; } = location;
    public string Channel { get; set; } = channel;
    public string Summary { get; set; } = summary;
    public string ManagerId { get; set; } = managerId;
    public double Budget { get; set; } = budget;
    public string JobCode { get; set; } = jobCode;
    public string Segment { get; set; } = segment;
    public SkillProfileRecordStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile record in status open.</summary>
    public static SkillProfileRecord NewSkillProfileRecord(string code, string name)
    {
        return new SkillProfileRecord(
            code,
            name,
            false,
            0,
            "",
            "",
            "",
            "",
            0.0,
            "",
            "",
            SkillProfileRecordStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileRecordStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileRecordStatus next)
    {
        switch (Status)
        {
            case SkillProfileRecordStatus.Open:
                return next == SkillProfileRecordStatus.Resolved || next == SkillProfileRecordStatus.Reopened;
            case SkillProfileRecordStatus.InReview:
                switch (next)
                {
                    case SkillProfileRecordStatus.Resolved:
                        return true;
                }
                return false;
            case SkillProfileRecordStatus.Resolved:
                return next == SkillProfileRecordStatus.Reopened || next == SkillProfileRecordStatus.Archived;
            case SkillProfileRecordStatus.Reopened:
                switch (next)
                {
                    case SkillProfileRecordStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"skill profile record"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Budget * (double)LeaveDays, 2);
    }

    public SkillProfileRecord Copy()
    {
        return new SkillProfileRecord(
            Code,
            Name,
            Pinned,
            LeaveDays,
            Location,
            Channel,
            Summary,
            ManagerId,
            Budget,
            JobCode,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
