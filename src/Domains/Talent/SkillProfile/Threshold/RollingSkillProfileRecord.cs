namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling skill profile record is in its life cycle.</summary>
public enum RollingSkillProfileRecordStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a rolling skill profile record and its current status.</summary>
public sealed class RollingSkillProfileRecord(string code, string name, double maximum, string costCenter, string employeeId, string category, double rate, RollingSkillProfileRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string CostCenter { get; set; } = costCenter;
    public string EmployeeId { get; set; } = employeeId;
    public string Category { get; set; } = category;
    public double Rate { get; set; } = rate;
    public RollingSkillProfileRecordStatus Status { get; set; } = status;

    /// <summary>Starts a rolling skill profile record in status requested.</summary>
    public static RollingSkillProfileRecord NewRollingSkillProfileRecord(string code, string name)
    {
        return new RollingSkillProfileRecord(
            code,
            name,
            0.0,
            "",
            "",
            "",
            0.0,
            RollingSkillProfileRecordStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingSkillProfileRecordStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingSkillProfileRecordStatus next)
    {
        switch (Status)
        {
            case RollingSkillProfileRecordStatus.Requested:
                return next == RollingSkillProfileRecordStatus.Granted || next == RollingSkillProfileRecordStatus.Expired;
            case RollingSkillProfileRecordStatus.Granted:
                return next == RollingSkillProfileRecordStatus.Revoked;
            case RollingSkillProfileRecordStatus.Revoked:
                switch (next)
                {
                    case RollingSkillProfileRecordStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingSkillProfileRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling skill profile record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Maximum - Rate, 2);
    }

    public RollingSkillProfileRecord Copy()
    {
        return new RollingSkillProfileRecord(Code, Name, Maximum, CostCenter, EmployeeId, Category, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
