namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict skill profile schedule is in its life cycle.</summary>
public enum StrictSkillProfileScheduleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A strict skill profile schedule as the skill profile team keeps it.</summary>
public sealed class StrictSkillProfileSchedule(string code, string name, string notes, bool visible, string jobCode, string phone, string externalRef, double fte, StrictSkillProfileScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public bool Visible { get; set; } = visible;
    public string JobCode { get; set; } = jobCode;
    public string Phone { get; set; } = phone;
    public string ExternalRef { get; set; } = externalRef;
    public double Fte { get; set; } = fte;
    public StrictSkillProfileScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a strict skill profile schedule in status draft.</summary>
    public static StrictSkillProfileSchedule NewStrictSkillProfileSchedule(string code, string name)
    {
        return new StrictSkillProfileSchedule(
            code,
            name,
            "",
            false,
            "",
            "",
            "",
            0.0,
            StrictSkillProfileScheduleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictSkillProfileScheduleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictSkillProfileScheduleStatus next)
    {
        switch (Status)
        {
            case StrictSkillProfileScheduleStatus.Draft:
                return next == StrictSkillProfileScheduleStatus.Active;
            case StrictSkillProfileScheduleStatus.Pending:
                return next == StrictSkillProfileScheduleStatus.Active || next == StrictSkillProfileScheduleStatus.Closed;
            case StrictSkillProfileScheduleStatus.Active:
                switch (next)
                {
                    case StrictSkillProfileScheduleStatus.Suspended:
                    case StrictSkillProfileScheduleStatus.Closed:
                        return true;
                }
                return false;
            case StrictSkillProfileScheduleStatus.Suspended:
                return next == StrictSkillProfileScheduleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(StrictSkillProfileScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict skill profile schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public StrictSkillProfileSchedule Copy()
    {
        return new StrictSkillProfileSchedule(Code, Name, Notes, Visible, JobCode, Phone, ExternalRef, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
