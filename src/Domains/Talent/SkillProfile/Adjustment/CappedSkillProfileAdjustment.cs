namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped skill profile adjustment is in its life cycle.</summary>
public enum CappedSkillProfileAdjustmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One capped skill profile adjustment, with what the planners need to act on it.</summary>
public sealed class CappedSkillProfileAdjustment(string code, string name, string salaryBand, string summary, int version, string managerId, int quantity, int windowDays, bool active, double floorValue, string segment, CappedSkillProfileAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public string Summary { get; set; } = summary;
    public int Version { get; set; } = version;
    public string ManagerId { get; set; } = managerId;
    public int Quantity { get; set; } = quantity;
    public int WindowDays { get; set; } = windowDays;
    public bool Active { get; set; } = active;
    public double FloorValue { get; set; } = floorValue;
    public string Segment { get; set; } = segment;
    public CappedSkillProfileAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a capped skill profile adjustment in status draft.</summary>
    public static CappedSkillProfileAdjustment NewCappedSkillProfileAdjustment(string code, string name)
    {
        return new CappedSkillProfileAdjustment(
            code,
            name,
            "",
            "",
            0,
            "",
            0,
            0,
            false,
            0.0,
            "",
            CappedSkillProfileAdjustmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedSkillProfileAdjustmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedSkillProfileAdjustmentStatus next)
    {
        switch (Status)
        {
            case CappedSkillProfileAdjustmentStatus.Draft:
                return next == CappedSkillProfileAdjustmentStatus.Pending;
            case CappedSkillProfileAdjustmentStatus.Pending:
                return next == CappedSkillProfileAdjustmentStatus.Active;
            case CappedSkillProfileAdjustmentStatus.Active:
                return next == CappedSkillProfileAdjustmentStatus.Suspended || next == CappedSkillProfileAdjustmentStatus.Closed;
            case CappedSkillProfileAdjustmentStatus.Suspended:
                return next == CappedSkillProfileAdjustmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CappedSkillProfileAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped skill profile adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"capped skill profile adjustment"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)WindowDays - (double)Version, 2);
    }

    public CappedSkillProfileAdjustment Copy()
    {
        return new CappedSkillProfileAdjustment(
            Code,
            Name,
            SalaryBand,
            Summary,
            Version,
            ManagerId,
            Quantity,
            WindowDays,
            Active,
            FloorValue,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
