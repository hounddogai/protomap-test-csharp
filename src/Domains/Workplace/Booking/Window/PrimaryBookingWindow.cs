namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary booking window is in its life cycle.</summary>
public enum PrimaryBookingWindowStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A primary booking window. Its code is unique within a tenant.</summary>
public sealed class PrimaryBookingWindow(string code, string name, int startsAt, double margin, string department, string skill, double ceiling, string phone, double cost, string currency, PrimaryBookingWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public double Margin { get; set; } = margin;
    public string Department { get; set; } = department;
    public string Skill { get; set; } = skill;
    public double Ceiling { get; set; } = ceiling;
    public string Phone { get; set; } = phone;
    public double Cost { get; set; } = cost;
    public string Currency { get; set; } = currency;
    public PrimaryBookingWindowStatus Status { get; set; } = status;

    /// <summary>Starts a primary booking window in status planned.</summary>
    public static PrimaryBookingWindow NewPrimaryBookingWindow(string code, string name)
    {
        return new PrimaryBookingWindow(
            code,
            name,
            0,
            0.0,
            "",
            "",
            0.0,
            "",
            0.0,
            "",
            PrimaryBookingWindowStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryBookingWindowStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryBookingWindowStatus next)
    {
        switch (Status)
        {
            case PrimaryBookingWindowStatus.Planned:
                return next == PrimaryBookingWindowStatus.Scheduled;
            case PrimaryBookingWindowStatus.Scheduled:
                switch (next)
                {
                    case PrimaryBookingWindowStatus.Completed:
                    case PrimaryBookingWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryBookingWindowStatus.InProgress:
                return next == PrimaryBookingWindowStatus.Completed || next == PrimaryBookingWindowStatus.Cancelled;
            case PrimaryBookingWindowStatus.Completed:
                switch (next)
                {
                    case PrimaryBookingWindowStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryBookingWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary booking window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetVolume()
    {
        return Math.Round(Margin - Cost, 2);
    }

    public PrimaryBookingWindow Copy()
    {
        return new PrimaryBookingWindow(
            Code,
            Name,
            StartsAt,
            Margin,
            Department,
            Skill,
            Ceiling,
            Phone,
            Cost,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
