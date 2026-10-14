namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking template is in its life cycle.</summary>
public enum BookingTemplateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One booking template, with what the planners need to act on it.</summary>
public sealed class BookingTemplate(string code, string name, string team, double ceiling, double hours, string unit, BookingTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public double Ceiling { get; set; } = ceiling;
    public double Hours { get; set; } = hours;
    public string Unit { get; set; } = unit;
    public BookingTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a booking template in status idle.</summary>
    public static BookingTemplate NewBookingTemplate(string code, string name)
    {
        return new BookingTemplate(code, name, "", 0.0, 0.0, "", BookingTemplateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingTemplateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingTemplateStatus next)
    {
        switch (Status)
        {
            case BookingTemplateStatus.Idle:
                return next == BookingTemplateStatus.Warming;
            case BookingTemplateStatus.Warming:
                switch (next)
                {
                    case BookingTemplateStatus.Draining:
                        return true;
                }
                return false;
            case BookingTemplateStatus.Ready:
                switch (next)
                {
                    case BookingTemplateStatus.Draining:
                        return true;
                }
                return false;
            case BookingTemplateStatus.Draining:
                switch (next)
                {
                    case BookingTemplateStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Hours + Ceiling, 2);
    }

    public BookingTemplate Copy()
    {
        return new BookingTemplate(Code, Name, Team, Ceiling, Hours, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
