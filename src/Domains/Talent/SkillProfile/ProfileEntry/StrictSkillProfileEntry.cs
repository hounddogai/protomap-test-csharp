namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict skill profile entry is in its life cycle.</summary>
public enum StrictSkillProfileEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A strict skill profile entry. Its code is unique within a tenant.</summary>
public sealed class StrictSkillProfileEntry(string code, string title, string salaryBand, string employeeId, double fte, string checksum, string team, bool archived, bool active, StrictSkillProfileEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public string EmployeeId { get; set; } = employeeId;
    public double Fte { get; set; } = fte;
    public string Checksum { get; set; } = checksum;
    public string Team { get; set; } = team;
    public bool Archived { get; set; } = archived;
    public bool Active { get; set; } = active;
    public StrictSkillProfileEntryStatus Status { get; set; } = status;

    /// <summary>Starts a strict skill profile entry in status planned.</summary>
    public static StrictSkillProfileEntry NewStrictSkillProfileEntry(string code, string title)
    {
        return new StrictSkillProfileEntry(
            code,
            title,
            "",
            "",
            0.0,
            "",
            "",
            false,
            false,
            StrictSkillProfileEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictSkillProfileEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictSkillProfileEntryStatus next)
    {
        switch (Status)
        {
            case StrictSkillProfileEntryStatus.Planned:
                return next == StrictSkillProfileEntryStatus.InProgress;
            case StrictSkillProfileEntryStatus.Scheduled:
                switch (next)
                {
                    case StrictSkillProfileEntryStatus.InProgress:
                        return true;
                }
                return false;
            case StrictSkillProfileEntryStatus.InProgress:
                switch (next)
                {
                    case StrictSkillProfileEntryStatus.Completed:
                    case StrictSkillProfileEntryStatus.Cancelled:
                        return true;
                }
                return false;
            case StrictSkillProfileEntryStatus.Completed:
                switch (next)
                {
                    case StrictSkillProfileEntryStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictSkillProfileEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict skill profile entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict skill profile entry"} {Code}";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public StrictSkillProfileEntry Copy()
    {
        return new StrictSkillProfileEntry(
            Code,
            Title,
            SalaryBand,
            EmployeeId,
            Fte,
            Checksum,
            Team,
            Archived,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
