namespace Acme.Workforce.Talent.SkillProfile.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary skill profile threshold is in its life cycle.</summary>
public enum PrimarySkillProfileThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A primary skill profile threshold as the skill profile team keeps it.</summary>
public sealed class PrimarySkillProfileThreshold(string code, string title, string label, string costCenter, bool pinned, string team, double cost, bool approved, int durationMinutes, int windowDays, PrimarySkillProfileThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public bool Pinned { get; set; } = pinned;
    public string Team { get; set; } = team;
    public double Cost { get; set; } = cost;
    public bool Approved { get; set; } = approved;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int WindowDays { get; set; } = windowDays;
    public PrimarySkillProfileThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a primary skill profile threshold in status draft.</summary>
    public static PrimarySkillProfileThreshold NewPrimarySkillProfileThreshold(string code, string title)
    {
        return new PrimarySkillProfileThreshold(
            code,
            title,
            "",
            "",
            false,
            "",
            0.0,
            false,
            0,
            0,
            PrimarySkillProfileThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimarySkillProfileThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimarySkillProfileThresholdStatus next)
    {
        switch (Status)
        {
            case PrimarySkillProfileThresholdStatus.Draft:
                return next == PrimarySkillProfileThresholdStatus.Pending || next == PrimarySkillProfileThresholdStatus.Closed;
            case PrimarySkillProfileThresholdStatus.Pending:
                switch (next)
                {
                    case PrimarySkillProfileThresholdStatus.Active:
                    case PrimarySkillProfileThresholdStatus.Closed:
                        return true;
                }
                return false;
            case PrimarySkillProfileThresholdStatus.Active:
                return next == PrimarySkillProfileThresholdStatus.Closed;
            case PrimarySkillProfileThresholdStatus.Suspended:
                switch (next)
                {
                    case PrimarySkillProfileThresholdStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimarySkillProfileThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary skill profile threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"primary skill profile threshold"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)WindowDays + (double)DurationMinutes, 2);
    }

    public PrimarySkillProfileThreshold Copy()
    {
        return new PrimarySkillProfileThreshold(
            Code,
            Title,
            Label,
            CostCenter,
            Pinned,
            Team,
            Cost,
            Approved,
            DurationMinutes,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
