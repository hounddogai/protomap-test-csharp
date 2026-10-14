namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking forecast is in its life cycle.</summary>
public enum BookingForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A booking forecast. Its code is unique within a tenant.</summary>
public sealed class BookingForecast(string code, string title, string currency, double weight, double score, bool visible, string costCenter, double minimum, double cost, string badge, BookingForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public double Weight { get; set; } = weight;
    public double Score { get; set; } = score;
    public bool Visible { get; set; } = visible;
    public string CostCenter { get; set; } = costCenter;
    public double Minimum { get; set; } = minimum;
    public double Cost { get; set; } = cost;
    public string Badge { get; set; } = badge;
    public BookingForecastStatus Status { get; set; } = status;

    /// <summary>Starts a booking forecast in status idle.</summary>
    public static BookingForecast NewBookingForecast(string code, string title)
    {
        return new BookingForecast(code, title, "", 0.0, 0.0, false, "", 0.0, 0.0, "", BookingForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingForecastStatus next)
    {
        switch (Status)
        {
            case BookingForecastStatus.Idle:
                return next == BookingForecastStatus.Draining || next == BookingForecastStatus.Stopped;
            case BookingForecastStatus.Warming:
                return next == BookingForecastStatus.Ready || next == BookingForecastStatus.Draining;
            case BookingForecastStatus.Ready:
                switch (next)
                {
                    case BookingForecastStatus.Draining:
                        return true;
                }
                return false;
            case BookingForecastStatus.Draining:
                switch (next)
                {
                    case BookingForecastStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Minimum - Score, 2);
    }

    public BookingForecast Copy()
    {
        return new BookingForecast(
            Code,
            Title,
            Currency,
            Weight,
            Score,
            Visible,
            CostCenter,
            Minimum,
            Cost,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
