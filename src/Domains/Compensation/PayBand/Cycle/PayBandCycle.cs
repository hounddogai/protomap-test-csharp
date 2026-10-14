namespace Acme.Workforce.Compensation.PayBand.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band cycle is in its life cycle.</summary>
public enum PayBandCycleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a pay band cycle and its current status.</summary>
public sealed class PayBandCycle(string code, string name, int quantity, double hours, string managerId, double floorValue, string skill, double cost, double discount, PayBandCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public double Hours { get; set; } = hours;
    public string ManagerId { get; set; } = managerId;
    public double FloorValue { get; set; } = floorValue;
    public string Skill { get; set; } = skill;
    public double Cost { get; set; } = cost;
    public double Discount { get; set; } = discount;
    public PayBandCycleStatus Status { get; set; } = status;

    /// <summary>Starts a pay band cycle in status new.</summary>
    public static PayBandCycle NewPayBandCycle(string code, string name)
    {
        return new PayBandCycle(code, name, 0, 0.0, "", 0.0, "", 0.0, 0.0, PayBandCycleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandCycleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandCycleStatus next)
    {
        switch (Status)
        {
            case PayBandCycleStatus.New:
                switch (next)
                {
                    case PayBandCycleStatus.Done:
                    case PayBandCycleStatus.Failed:
                        return true;
                }
                return false;
            case PayBandCycleStatus.Queued:
                return next == PayBandCycleStatus.Running || next == PayBandCycleStatus.Done;
            case PayBandCycleStatus.Running:
                return next == PayBandCycleStatus.Done || next == PayBandCycleStatus.Failed;
            case PayBandCycleStatus.Done:
                switch (next)
                {
                    case PayBandCycleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)Quantity * Hours, 2);
    }

    public PayBandCycle Copy()
    {
        return new PayBandCycle(Code, Name, Quantity, Hours, ManagerId, FloorValue, Skill, Cost, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
