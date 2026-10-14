namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional skill profile group is in its life cycle.</summary>
public enum RegionalSkillProfileGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One regional skill profile group, with what the planners need to act on it.</summary>
public sealed class RegionalSkillProfileGroup(string code, string label, double budget, string grade, string channel, string jobCode, int durationMinutes, RegionalSkillProfileGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string Grade { get; set; } = grade;
    public string Channel { get; set; } = channel;
    public string JobCode { get; set; } = jobCode;
    public int DurationMinutes { get; set; } = durationMinutes;
    public RegionalSkillProfileGroupStatus Status { get; set; } = status;

    /// <summary>Starts a regional skill profile group in status created.</summary>
    public static RegionalSkillProfileGroup NewRegionalSkillProfileGroup(string code, string label)
    {
        return new RegionalSkillProfileGroup(
            code,
            label,
            0.0,
            "",
            "",
            "",
            0,
            RegionalSkillProfileGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSkillProfileGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSkillProfileGroupStatus next)
    {
        switch (Status)
        {
            case RegionalSkillProfileGroupStatus.Created:
                return next == RegionalSkillProfileGroupStatus.Published;
            case RegionalSkillProfileGroupStatus.Validated:
                switch (next)
                {
                    case RegionalSkillProfileGroupStatus.Published:
                    case RegionalSkillProfileGroupStatus.Retired:
                        return true;
                }
                return false;
            case RegionalSkillProfileGroupStatus.Published:
                return next == RegionalSkillProfileGroupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RegionalSkillProfileGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional skill profile group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Budget + (double)DurationMinutes, 2);
    }

    public RegionalSkillProfileGroup Copy()
    {
        return new RegionalSkillProfileGroup(Code, Label, Budget, Grade, Channel, JobCode, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
