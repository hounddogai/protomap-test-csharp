namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual skill profile exception is in its life cycle.</summary>
public enum ManualSkillProfileExceptionStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a manual skill profile exception and its current status.</summary>
public sealed class ManualSkillProfileException(string code, string name, string category, bool pinned, string employeeId, string department, string label, double threshold, string team, ManualSkillProfileExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public bool Pinned { get; set; } = pinned;
    public string EmployeeId { get; set; } = employeeId;
    public string Department { get; set; } = department;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public string Team { get; set; } = team;
    public ManualSkillProfileExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a manual skill profile exception in status new.</summary>
    public static ManualSkillProfileException NewManualSkillProfileException(string code, string name)
    {
        return new ManualSkillProfileException(
            code,
            name,
            "",
            false,
            "",
            "",
            "",
            0.0,
            "",
            ManualSkillProfileExceptionStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualSkillProfileExceptionStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualSkillProfileExceptionStatus next)
    {
        switch (Status)
        {
            case ManualSkillProfileExceptionStatus.New:
                return next == ManualSkillProfileExceptionStatus.Running;
            case ManualSkillProfileExceptionStatus.Queued:
                return next == ManualSkillProfileExceptionStatus.Running || next == ManualSkillProfileExceptionStatus.Failed;
            case ManualSkillProfileExceptionStatus.Running:
                switch (next)
                {
                    case ManualSkillProfileExceptionStatus.Done:
                    case ManualSkillProfileExceptionStatus.Failed:
                        return true;
                }
                return false;
            case ManualSkillProfileExceptionStatus.Done:
                return next == ManualSkillProfileExceptionStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ManualSkillProfileExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual skill profile exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"manual skill profile exception"} {Code}";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public ManualSkillProfileException Copy()
    {
        return new ManualSkillProfileException(
            Code,
            Name,
            Category,
            Pinned,
            EmployeeId,
            Department,
            Label,
            Threshold,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
