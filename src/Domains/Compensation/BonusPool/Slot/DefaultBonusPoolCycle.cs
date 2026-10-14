namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default bonus pool cycle is in its life cycle.</summary>
public enum DefaultBonusPoolCycleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A default bonus pool cycle as the bonus pool team keeps it.</summary>
public sealed class DefaultBonusPoolCycle(string code, string label, double discount, string skill, string employeeId, DefaultBonusPoolCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public string Skill { get; set; } = skill;
    public string EmployeeId { get; set; } = employeeId;
    public DefaultBonusPoolCycleStatus Status { get; set; } = status;

    /// <summary>Starts a default bonus pool cycle in status created.</summary>
    public static DefaultBonusPoolCycle NewDefaultBonusPoolCycle(string code, string label)
    {
        return new DefaultBonusPoolCycle(code, label, 0.0, "", "", DefaultBonusPoolCycleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultBonusPoolCycleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultBonusPoolCycleStatus next)
    {
        switch (Status)
        {
            case DefaultBonusPoolCycleStatus.Created:
                return next == DefaultBonusPoolCycleStatus.Published || next == DefaultBonusPoolCycleStatus.Retired;
            case DefaultBonusPoolCycleStatus.Validated:
                switch (next)
                {
                    case DefaultBonusPoolCycleStatus.Published:
                    case DefaultBonusPoolCycleStatus.Retired:
                        return true;
                }
                return false;
            case DefaultBonusPoolCycleStatus.Published:
                switch (next)
                {
                    case DefaultBonusPoolCycleStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultBonusPoolCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default bonus pool cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public DefaultBonusPoolCycle Copy()
    {
        return new DefaultBonusPoolCycle(Code, Label, Discount, Skill, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
