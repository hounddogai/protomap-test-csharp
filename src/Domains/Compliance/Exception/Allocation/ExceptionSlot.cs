namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception slot is in its life cycle.</summary>
public enum ExceptionSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A exception slot. Its code is unique within a tenant.</summary>
public sealed class ExceptionSlot(string code, string name, string department, int durationMinutes, double weight, double minimum, string phone, ExceptionSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Weight { get; set; } = weight;
    public double Minimum { get; set; } = minimum;
    public string Phone { get; set; } = phone;
    public ExceptionSlotStatus Status { get; set; } = status;

    /// <summary>Starts a exception slot in status proposed.</summary>
    public static ExceptionSlot NewExceptionSlot(string code, string name)
    {
        return new ExceptionSlot(code, name, "", 0, 0.0, 0.0, "", ExceptionSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionSlotStatus next)
    {
        switch (Status)
        {
            case ExceptionSlotStatus.Proposed:
                return next == ExceptionSlotStatus.Rejected || next == ExceptionSlotStatus.Withdrawn;
            case ExceptionSlotStatus.Approved:
                return next == ExceptionSlotStatus.Rejected || next == ExceptionSlotStatus.Withdrawn;
            case ExceptionSlotStatus.Rejected:
                switch (next)
                {
                    case ExceptionSlotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"exception slot"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Minimum - Weight, 2);
    }

    public ExceptionSlot Copy()
    {
        return new ExceptionSlot(Code, Name, Department, DurationMinutes, Weight, Minimum, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
