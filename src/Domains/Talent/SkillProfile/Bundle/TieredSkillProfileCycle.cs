namespace Acme.Workforce.Talent.SkillProfile.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered skill profile cycle is in its life cycle.</summary>
public enum TieredSkillProfileCycleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a tiered skill profile cycle and its current status.</summary>
public sealed class TieredSkillProfileCycle(string code, string label, int attempts, string timezone, int leaveDays, string salaryBand, TieredSkillProfileCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public string Timezone { get; set; } = timezone;
    public int LeaveDays { get; set; } = leaveDays;
    public string SalaryBand { get; set; } = salaryBand;
    public TieredSkillProfileCycleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered skill profile cycle in status draft.</summary>
    public static TieredSkillProfileCycle NewTieredSkillProfileCycle(string code, string label)
    {
        return new TieredSkillProfileCycle(code, label, 0, "", 0, "", TieredSkillProfileCycleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredSkillProfileCycleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredSkillProfileCycleStatus next)
    {
        switch (Status)
        {
            case TieredSkillProfileCycleStatus.Draft:
                switch (next)
                {
                    case TieredSkillProfileCycleStatus.Active:
                        return true;
                }
                return false;
            case TieredSkillProfileCycleStatus.Pending:
                return next == TieredSkillProfileCycleStatus.Active || next == TieredSkillProfileCycleStatus.Closed;
            case TieredSkillProfileCycleStatus.Active:
                return next == TieredSkillProfileCycleStatus.Suspended;
            case TieredSkillProfileCycleStatus.Suspended:
                switch (next)
                {
                    case TieredSkillProfileCycleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredSkillProfileCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered skill profile cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetValue()
    {
        return Math.Round((double)LeaveDays + (double)Attempts, 2);
    }

    public TieredSkillProfileCycle Copy()
    {
        return new TieredSkillProfileCycle(Code, Label, Attempts, Timezone, LeaveDays, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
