namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile schedule is in its life cycle.</summary>
public enum SkillProfileScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a skill profile schedule and its current status.</summary>
public sealed class SkillProfileSchedule(string code, string name, int startsAt, int attempts, int leaveDays, string checksum, double threshold, SkillProfileScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public int Attempts { get; set; } = attempts;
    public int LeaveDays { get; set; } = leaveDays;
    public string Checksum { get; set; } = checksum;
    public double Threshold { get; set; } = threshold;
    public SkillProfileScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile schedule in status planned.</summary>
    public static SkillProfileSchedule NewSkillProfileSchedule(string code, string name)
    {
        return new SkillProfileSchedule(code, name, 0, 0, 0, "", 0.0, SkillProfileScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileScheduleStatus next)
    {
        switch (Status)
        {
            case SkillProfileScheduleStatus.Planned:
                switch (next)
                {
                    case SkillProfileScheduleStatus.Cancelled:
                        return true;
                }
                return false;
            case SkillProfileScheduleStatus.Scheduled:
                return next == SkillProfileScheduleStatus.Completed || next == SkillProfileScheduleStatus.Cancelled;
            case SkillProfileScheduleStatus.InProgress:
                return next == SkillProfileScheduleStatus.Cancelled;
            case SkillProfileScheduleStatus.Completed:
                switch (next)
                {
                    case SkillProfileScheduleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"skill profile schedule"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Attempts + (double)StartsAt, 2);
    }

    public SkillProfileSchedule Copy()
    {
        return new SkillProfileSchedule(Code, Name, StartsAt, Attempts, LeaveDays, Checksum, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
