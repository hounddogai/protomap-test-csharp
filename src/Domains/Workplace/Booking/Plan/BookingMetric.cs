namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking metric is in its life cycle.</summary>
public enum BookingMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A booking metric as the booking team keeps it.</summary>
public sealed class BookingMetric(string code, string name, int retries, string notes, string managerId, string category, int version, double amount, BookingMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string Notes { get; set; } = notes;
    public string ManagerId { get; set; } = managerId;
    public string Category { get; set; } = category;
    public int Version { get; set; } = version;
    public double Amount { get; set; } = amount;
    public BookingMetricStatus Status { get; set; } = status;

    /// <summary>Starts a booking metric in status requested.</summary>
    public static BookingMetric NewBookingMetric(string code, string name)
    {
        return new BookingMetric(code, name, 0, "", "", "", 0, 0.0, BookingMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingMetricStatus next)
    {
        switch (Status)
        {
            case BookingMetricStatus.Requested:
                switch (next)
                {
                    case BookingMetricStatus.Expired:
                        return true;
                }
                return false;
            case BookingMetricStatus.Granted:
                switch (next)
                {
                    case BookingMetricStatus.Expired:
                        return true;
                }
                return false;
            case BookingMetricStatus.Revoked:
                return next == BookingMetricStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BookingMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)Version * Amount, 2);
    }

    public BookingMetric Copy()
    {
        return new BookingMetric(Code, Name, Retries, Notes, ManagerId, Category, Version, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
