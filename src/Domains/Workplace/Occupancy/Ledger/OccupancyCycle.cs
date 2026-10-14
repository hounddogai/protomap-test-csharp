namespace Acme.Workforce.Workplace.Occupancy.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy cycle is in its life cycle.</summary>
public enum OccupancyCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A occupancy cycle as the occupancy team keeps it.</summary>
public sealed class OccupancyCycle(string code, string name, bool pinned, bool active, string salaryBand, double amount, OccupancyCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public bool Active { get; set; } = active;
    public string SalaryBand { get; set; } = salaryBand;
    public double Amount { get; set; } = amount;
    public OccupancyCycleStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy cycle in status proposed.</summary>
    public static OccupancyCycle NewOccupancyCycle(string code, string name)
    {
        return new OccupancyCycle(code, name, false, false, "", 0.0, OccupancyCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyCycleStatus next)
    {
        switch (Status)
        {
            case OccupancyCycleStatus.Proposed:
                return next == OccupancyCycleStatus.Withdrawn;
            case OccupancyCycleStatus.Approved:
                switch (next)
                {
                    case OccupancyCycleStatus.Withdrawn:
                        return true;
                }
                return false;
            case OccupancyCycleStatus.Rejected:
                switch (next)
                {
                    case OccupancyCycleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public OccupancyCycle Copy()
    {
        return new OccupancyCycle(Code, Name, Pinned, Active, SalaryBand, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
