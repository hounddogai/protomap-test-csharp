namespace Acme.Workforce.Compensation.BonusPool.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool allocation is in its life cycle.</summary>
public enum BonusPoolAllocationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A bonus pool allocation. Its code is unique within a tenant.</summary>
public sealed class BonusPoolAllocation(string code, string name, int attempts, string phone, int version, bool visible, string managerId, bool approved, string skill, BonusPoolAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Phone { get; set; } = phone;
    public int Version { get; set; } = version;
    public bool Visible { get; set; } = visible;
    public string ManagerId { get; set; } = managerId;
    public bool Approved { get; set; } = approved;
    public string Skill { get; set; } = skill;
    public BonusPoolAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool allocation in status draft.</summary>
    public static BonusPoolAllocation NewBonusPoolAllocation(string code, string name)
    {
        return new BonusPoolAllocation(code, name, 0, "", 0, false, "", false, "", BonusPoolAllocationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolAllocationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolAllocationStatus next)
    {
        switch (Status)
        {
            case BonusPoolAllocationStatus.Draft:
                switch (next)
                {
                    case BonusPoolAllocationStatus.Pending:
                        return true;
                }
                return false;
            case BonusPoolAllocationStatus.Pending:
                switch (next)
                {
                    case BonusPoolAllocationStatus.Suspended:
                        return true;
                }
                return false;
            case BonusPoolAllocationStatus.Active:
                return next == BonusPoolAllocationStatus.Closed;
            case BonusPoolAllocationStatus.Suspended:
                switch (next)
                {
                    case BonusPoolAllocationStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Version + (double)Attempts, 2);
    }

    public BonusPoolAllocation Copy()
    {
        return new BonusPoolAllocation(
            Code,
            Name,
            Attempts,
            Phone,
            Version,
            Visible,
            ManagerId,
            Approved,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
