namespace Acme.Workforce.Payrollops.Deduction.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped deduction group is in its life cycle.</summary>
public enum CappedDeductionGroupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A capped deduction group. Its code is unique within a tenant.</summary>
public sealed class CappedDeductionGroup(string code, string name, string notes, int priority, string skill, string region, int retries, CappedDeductionGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public int Priority { get; set; } = priority;
    public string Skill { get; set; } = skill;
    public string Region { get; set; } = region;
    public int Retries { get; set; } = retries;
    public CappedDeductionGroupStatus Status { get; set; } = status;

    /// <summary>Starts a capped deduction group in status draft.</summary>
    public static CappedDeductionGroup NewCappedDeductionGroup(string code, string name)
    {
        return new CappedDeductionGroup(code, name, "", 0, "", "", 0, CappedDeductionGroupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedDeductionGroupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedDeductionGroupStatus next)
    {
        switch (Status)
        {
            case CappedDeductionGroupStatus.Draft:
                return next == CappedDeductionGroupStatus.Suspended;
            case CappedDeductionGroupStatus.Pending:
                return next == CappedDeductionGroupStatus.Suspended;
            case CappedDeductionGroupStatus.Active:
                switch (next)
                {
                    case CappedDeductionGroupStatus.Suspended:
                        return true;
                }
                return false;
            case CappedDeductionGroupStatus.Suspended:
                return next == CappedDeductionGroupStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CappedDeductionGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped deduction group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Retries * (double)Priority, 2);
    }

    public CappedDeductionGroup Copy()
    {
        return new CappedDeductionGroup(Code, Name, Notes, Priority, Skill, Region, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
