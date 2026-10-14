namespace Acme.Workforce.Workplace.Booking.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking override is in its life cycle.</summary>
public enum BookingOverrideStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One booking override, with what the planners need to act on it.</summary>
public sealed class BookingOverride(string code, string label, double fte, double cost, string category, double budget, double discount, int graceDays, BookingOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public double Cost { get; set; } = cost;
    public string Category { get; set; } = category;
    public double Budget { get; set; } = budget;
    public double Discount { get; set; } = discount;
    public int GraceDays { get; set; } = graceDays;
    public BookingOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a booking override in status created.</summary>
    public static BookingOverride NewBookingOverride(string code, string label)
    {
        return new BookingOverride(code, label, 0.0, 0.0, "", 0.0, 0.0, 0, BookingOverrideStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingOverrideStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingOverrideStatus next)
    {
        switch (Status)
        {
            case BookingOverrideStatus.Created:
                switch (next)
                {
                    case BookingOverrideStatus.Published:
                    case BookingOverrideStatus.Retired:
                        return true;
                }
                return false;
            case BookingOverrideStatus.Validated:
                switch (next)
                {
                    case BookingOverrideStatus.Retired:
                        return true;
                }
                return false;
            case BookingOverrideStatus.Published:
                switch (next)
                {
                    case BookingOverrideStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Cost * (double)GraceDays, 2);
    }

    public BookingOverride Copy()
    {
        return new BookingOverride(Code, Label, Fte, Cost, Category, Budget, Discount, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
