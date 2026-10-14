namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile threshold is in its life cycle.</summary>
public enum SkillProfileThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a skill profile threshold and its current status.</summary>
public sealed class SkillProfileThreshold(string code, string label, int capacity, string grade, double minimum, SkillProfileThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public string Grade { get; set; } = grade;
    public double Minimum { get; set; } = minimum;
    public SkillProfileThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile threshold in status draft.</summary>
    public static SkillProfileThreshold NewSkillProfileThreshold(string code, string label)
    {
        return new SkillProfileThreshold(code, label, 0, "", 0.0, SkillProfileThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileThresholdStatus next)
    {
        switch (Status)
        {
            case SkillProfileThresholdStatus.Draft:
                switch (next)
                {
                    case SkillProfileThresholdStatus.Closed:
                        return true;
                }
                return false;
            case SkillProfileThresholdStatus.Pending:
                switch (next)
                {
                    case SkillProfileThresholdStatus.Suspended:
                    case SkillProfileThresholdStatus.Closed:
                        return true;
                }
                return false;
            case SkillProfileThresholdStatus.Active:
                switch (next)
                {
                    case SkillProfileThresholdStatus.Closed:
                        return true;
                }
                return false;
            case SkillProfileThresholdStatus.Suspended:
                switch (next)
                {
                    case SkillProfileThresholdStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetTotal()
    {
        return Math.Round((double)Capacity + Minimum, 2);
    }

    public SkillProfileThreshold Copy()
    {
        return new SkillProfileThreshold(Code, Label, Capacity, Grade, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
