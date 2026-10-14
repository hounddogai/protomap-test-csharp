namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling rate slot is in its life cycle.</summary>
public enum RollingRateSlotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A rolling rate slot as the rate team keeps it.</summary>
public sealed class RollingRateSlot(string code, string label, double ratio, int attempts, int limit, string managerId, RollingRateSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public int Attempts { get; set; } = attempts;
    public int Limit { get; set; } = limit;
    public string ManagerId { get; set; } = managerId;
    public RollingRateSlotStatus Status { get; set; } = status;

    /// <summary>Starts a rolling rate slot in status planned.</summary>
    public static RollingRateSlot NewRollingRateSlot(string code, string label)
    {
        return new RollingRateSlot(code, label, 0.0, 0, 0, "", RollingRateSlotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRateSlotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRateSlotStatus next)
    {
        switch (Status)
        {
            case RollingRateSlotStatus.Planned:
                return next == RollingRateSlotStatus.Scheduled || next == RollingRateSlotStatus.InProgress;
            case RollingRateSlotStatus.Scheduled:
                switch (next)
                {
                    case RollingRateSlotStatus.Completed:
                        return true;
                }
                return false;
            case RollingRateSlotStatus.InProgress:
                switch (next)
                {
                    case RollingRateSlotStatus.Completed:
                    case RollingRateSlotStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingRateSlotStatus.Completed:
                switch (next)
                {
                    case RollingRateSlotStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingRateSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling rate slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling rate slot"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Ratio * (double)Attempts, 2);
    }

    public RollingRateSlot Copy()
    {
        return new RollingRateSlot(Code, Label, Ratio, Attempts, Limit, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
