namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile rollup is in its life cycle.</summary>
public enum SkillProfileRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A skill profile rollup. Its code is unique within a tenant.</summary>
public sealed class SkillProfileRollup(string code, string title, string location, int startsAt, int graceDays, int createdAt, string summary, string checksum, bool archived, string jobCode, SkillProfileRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public int StartsAt { get; set; } = startsAt;
    public int GraceDays { get; set; } = graceDays;
    public int CreatedAt { get; set; } = createdAt;
    public string Summary { get; set; } = summary;
    public string Checksum { get; set; } = checksum;
    public bool Archived { get; set; } = archived;
    public string JobCode { get; set; } = jobCode;
    public SkillProfileRollupStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile rollup in status planned.</summary>
    public static SkillProfileRollup NewSkillProfileRollup(string code, string title)
    {
        return new SkillProfileRollup(code, title, "", 0, 0, 0, "", "", false, "", SkillProfileRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileRollupStatus next)
    {
        switch (Status)
        {
            case SkillProfileRollupStatus.Planned:
                switch (next)
                {
                    case SkillProfileRollupStatus.InProgress:
                        return true;
                }
                return false;
            case SkillProfileRollupStatus.Scheduled:
                return next == SkillProfileRollupStatus.Cancelled;
            case SkillProfileRollupStatus.InProgress:
                switch (next)
                {
                    case SkillProfileRollupStatus.Cancelled:
                        return true;
                }
                return false;
            case SkillProfileRollupStatus.Completed:
                switch (next)
                {
                    case SkillProfileRollupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"skill profile rollup"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)StartsAt * (double)CreatedAt, 2);
    }

    public SkillProfileRollup Copy()
    {
        return new SkillProfileRollup(
            Code,
            Title,
            Location,
            StartsAt,
            GraceDays,
            CreatedAt,
            Summary,
            Checksum,
            Archived,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
