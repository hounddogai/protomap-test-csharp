namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool cycle is in its life cycle.</summary>
public enum BonusPoolCycleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A bonus pool cycle. Its code is unique within a tenant.</summary>
public sealed class BonusPoolCycle(string code, string label, string unit, int graceDays, string managerId, string department, double floorValue, BonusPoolCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public int GraceDays { get; set; } = graceDays;
    public string ManagerId { get; set; } = managerId;
    public string Department { get; set; } = department;
    public double FloorValue { get; set; } = floorValue;
    public BonusPoolCycleStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool cycle in status requested.</summary>
    public static BonusPoolCycle NewBonusPoolCycle(string code, string label)
    {
        return new BonusPoolCycle(code, label, "", 0, "", "", 0.0, BonusPoolCycleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolCycleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolCycleStatus next)
    {
        switch (Status)
        {
            case BonusPoolCycleStatus.Requested:
                switch (next)
                {
                    case BonusPoolCycleStatus.Revoked:
                        return true;
                }
                return false;
            case BonusPoolCycleStatus.Granted:
                return next == BonusPoolCycleStatus.Revoked || next == BonusPoolCycleStatus.Expired;
            case BonusPoolCycleStatus.Revoked:
                return next == BonusPoolCycleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BonusPoolCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"bonus pool cycle"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(FloorValue + (double)GraceDays, 2);
    }

    public BonusPoolCycle Copy()
    {
        return new BonusPoolCycle(Code, Label, Unit, GraceDays, ManagerId, Department, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
