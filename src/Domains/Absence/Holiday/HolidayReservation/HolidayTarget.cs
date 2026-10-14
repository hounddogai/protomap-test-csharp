namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday target is in its life cycle.</summary>
public enum HolidayTargetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A holiday target as the holiday team keeps it.</summary>
public sealed class HolidayTarget(string code, string name, int retries, string skill, int leaveDays, List<string> tags, HolidayTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string Skill { get; set; } = skill;
    public int LeaveDays { get; set; } = leaveDays;
    public List<string> Tags { get; set; } = tags;
    public HolidayTargetStatus Status { get; set; } = status;

    /// <summary>Starts a holiday target in status idle.</summary>
    public static HolidayTarget NewHolidayTarget(string code, string name)
    {
        return new HolidayTarget(code, name, 0, "", 0, new List<string>(), HolidayTargetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayTargetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayTargetStatus next)
    {
        switch (Status)
        {
            case HolidayTargetStatus.Idle:
                switch (next)
                {
                    case HolidayTargetStatus.Ready:
                    case HolidayTargetStatus.Stopped:
                        return true;
                }
                return false;
            case HolidayTargetStatus.Warming:
                switch (next)
                {
                    case HolidayTargetStatus.Ready:
                        return true;
                }
                return false;
            case HolidayTargetStatus.Ready:
                switch (next)
                {
                    case HolidayTargetStatus.Stopped:
                        return true;
                }
                return false;
            case HolidayTargetStatus.Draining:
                switch (next)
                {
                    case HolidayTargetStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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
        return Math.Round((double)LeaveDays + (double)Retries, 2);
    }

    public HolidayTarget Copy()
    {
        return new HolidayTarget(Code, Name, Retries, Skill, LeaveDays, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
