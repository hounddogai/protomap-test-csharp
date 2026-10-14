namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default refresh slot is in its life cycle.</summary>
public enum DefaultRefreshSlotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a default refresh slot and its current status.</summary>
public sealed class DefaultRefreshSlot(string code, string name, bool active, string salaryBand, double discount, int revision, DefaultRefreshSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public string SalaryBand { get; set; } = salaryBand;
    public double Discount { get; set; } = discount;
    public int Revision { get; set; } = revision;
    public DefaultRefreshSlotStatus Status { get; set; } = status;

    /// <summary>Starts a default refresh slot in status idle.</summary>
    public static DefaultRefreshSlot NewDefaultRefreshSlot(string code, string name)
    {
        return new DefaultRefreshSlot(code, name, false, "", 0.0, 0, DefaultRefreshSlotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultRefreshSlotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultRefreshSlotStatus next)
    {
        switch (Status)
        {
            case DefaultRefreshSlotStatus.Idle:
                return next == DefaultRefreshSlotStatus.Warming || next == DefaultRefreshSlotStatus.Draining;
            case DefaultRefreshSlotStatus.Warming:
                switch (next)
                {
                    case DefaultRefreshSlotStatus.Ready:
                    case DefaultRefreshSlotStatus.Draining:
                        return true;
                }
                return false;
            case DefaultRefreshSlotStatus.Ready:
                switch (next)
                {
                    case DefaultRefreshSlotStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultRefreshSlotStatus.Draining:
                switch (next)
                {
                    case DefaultRefreshSlotStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultRefreshSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default refresh slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"default refresh slot"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Discount - (double)Revision, 2);
    }

    public DefaultRefreshSlot Copy()
    {
        return new DefaultRefreshSlot(Code, Name, Active, SalaryBand, Discount, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
