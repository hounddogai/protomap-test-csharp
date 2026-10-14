namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional skill profile record is in its life cycle.</summary>
public enum RegionalSkillProfileRecordStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A regional skill profile record as the skill profile team keeps it.</summary>
public sealed class RegionalSkillProfileRecord(string code, string label, string team, int windowDays, double floorValue, string checksum, int quantity, RegionalSkillProfileRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public int WindowDays { get; set; } = windowDays;
    public double FloorValue { get; set; } = floorValue;
    public string Checksum { get; set; } = checksum;
    public int Quantity { get; set; } = quantity;
    public RegionalSkillProfileRecordStatus Status { get; set; } = status;

    /// <summary>Starts a regional skill profile record in status new.</summary>
    public static RegionalSkillProfileRecord NewRegionalSkillProfileRecord(string code, string label)
    {
        return new RegionalSkillProfileRecord(code, label, "", 0, 0.0, "", 0, RegionalSkillProfileRecordStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSkillProfileRecordStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSkillProfileRecordStatus next)
    {
        switch (Status)
        {
            case RegionalSkillProfileRecordStatus.New:
                switch (next)
                {
                    case RegionalSkillProfileRecordStatus.Queued:
                    case RegionalSkillProfileRecordStatus.Running:
                        return true;
                }
                return false;
            case RegionalSkillProfileRecordStatus.Queued:
                switch (next)
                {
                    case RegionalSkillProfileRecordStatus.Running:
                        return true;
                }
                return false;
            case RegionalSkillProfileRecordStatus.Running:
                switch (next)
                {
                    case RegionalSkillProfileRecordStatus.Failed:
                        return true;
                }
                return false;
            case RegionalSkillProfileRecordStatus.Done:
                return next == RegionalSkillProfileRecordStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RegionalSkillProfileRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional skill profile record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)WindowDays * FloorValue, 2);
    }

    public RegionalSkillProfileRecord Copy()
    {
        return new RegionalSkillProfileRecord(Code, Label, Team, WindowDays, FloorValue, Checksum, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
