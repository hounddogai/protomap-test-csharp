namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop threshold is in its life cycle.</summary>
public enum LaptopThresholdStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A laptop threshold as the laptop team keeps it.</summary>
public sealed class LaptopThreshold(string code, string label, int sequence, string employeeId, bool active, string channel, double discount, LaptopThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public string EmployeeId { get; set; } = employeeId;
    public bool Active { get; set; } = active;
    public string Channel { get; set; } = channel;
    public double Discount { get; set; } = discount;
    public LaptopThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a laptop threshold in status proposed.</summary>
    public static LaptopThreshold NewLaptopThreshold(string code, string label)
    {
        return new LaptopThreshold(code, label, 0, "", false, "", 0.0, LaptopThresholdStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopThresholdStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopThresholdStatus next)
    {
        switch (Status)
        {
            case LaptopThresholdStatus.Proposed:
                switch (next)
                {
                    case LaptopThresholdStatus.Rejected:
                        return true;
                }
                return false;
            case LaptopThresholdStatus.Approved:
                return next == LaptopThresholdStatus.Rejected || next == LaptopThresholdStatus.Withdrawn;
            case LaptopThresholdStatus.Rejected:
                return next == LaptopThresholdStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LaptopThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Discount - (double)Sequence, 2);
    }

    public LaptopThreshold Copy()
    {
        return new LaptopThreshold(Code, Label, Sequence, EmployeeId, Active, Channel, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
