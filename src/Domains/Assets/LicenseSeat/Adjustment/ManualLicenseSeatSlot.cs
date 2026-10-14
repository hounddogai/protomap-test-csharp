namespace Acme.Workforce.Assets.LicenseSeat.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual license seat slot is in its life cycle.</summary>
public enum ManualLicenseSeatSlotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One manual license seat slot, with what the planners need to act on it.</summary>
public sealed class ManualLicenseSeatSlot(string code, string name, double hours, int durationMinutes, string region, ManualLicenseSeatSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Region { get; set; } = region;
    public ManualLicenseSeatSlotStatus Status { get; set; } = status;

    /// <summary>Starts a manual license seat slot in status idle.</summary>
    public static ManualLicenseSeatSlot NewManualLicenseSeatSlot(string code, string name)
    {
        return new ManualLicenseSeatSlot(code, name, 0.0, 0, "", ManualLicenseSeatSlotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualLicenseSeatSlotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualLicenseSeatSlotStatus next)
    {
        switch (Status)
        {
            case ManualLicenseSeatSlotStatus.Idle:
                return next == ManualLicenseSeatSlotStatus.Ready || next == ManualLicenseSeatSlotStatus.Draining;
            case ManualLicenseSeatSlotStatus.Warming:
                switch (next)
                {
                    case ManualLicenseSeatSlotStatus.Ready:
                    case ManualLicenseSeatSlotStatus.Stopped:
                        return true;
                }
                return false;
            case ManualLicenseSeatSlotStatus.Ready:
                switch (next)
                {
                    case ManualLicenseSeatSlotStatus.Stopped:
                        return true;
                }
                return false;
            case ManualLicenseSeatSlotStatus.Draining:
                switch (next)
                {
                    case ManualLicenseSeatSlotStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualLicenseSeatSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual license seat slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Hours - (double)DurationMinutes, 2);
    }

    public ManualLicenseSeatSlot Copy()
    {
        return new ManualLicenseSeatSlot(Code, Name, Hours, DurationMinutes, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
