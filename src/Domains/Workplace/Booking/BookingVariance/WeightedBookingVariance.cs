namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted booking variance is in its life cycle.</summary>
public enum WeightedBookingVarianceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A weighted booking variance. Its code is unique within a tenant.</summary>
public sealed class WeightedBookingVariance(string code, string name, string externalRef, double floorValue, int priority, string channel, double maximum, int graceDays, double rating, string country, double ratio, WeightedBookingVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public double FloorValue { get; set; } = floorValue;
    public int Priority { get; set; } = priority;
    public string Channel { get; set; } = channel;
    public double Maximum { get; set; } = maximum;
    public int GraceDays { get; set; } = graceDays;
    public double Rating { get; set; } = rating;
    public string Country { get; set; } = country;
    public double Ratio { get; set; } = ratio;
    public WeightedBookingVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a weighted booking variance in status requested.</summary>
    public static WeightedBookingVariance NewWeightedBookingVariance(string code, string name)
    {
        return new WeightedBookingVariance(
            code,
            name,
            "",
            0.0,
            0,
            "",
            0.0,
            0,
            0.0,
            "",
            0.0,
            WeightedBookingVarianceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedBookingVarianceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedBookingVarianceStatus next)
    {
        switch (Status)
        {
            case WeightedBookingVarianceStatus.Requested:
                return next == WeightedBookingVarianceStatus.Revoked;
            case WeightedBookingVarianceStatus.Granted:
                switch (next)
                {
                    case WeightedBookingVarianceStatus.Revoked:
                        return true;
                }
                return false;
            case WeightedBookingVarianceStatus.Revoked:
                return next == WeightedBookingVarianceStatus.Expired;
        }
        return false;
    }

    public void MoveTo(WeightedBookingVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted booking variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetLoad()
    {
        return Math.Round(FloorValue - (double)GraceDays, 2);
    }

    public WeightedBookingVariance Copy()
    {
        return new WeightedBookingVariance(
            Code,
            Name,
            ExternalRef,
            FloorValue,
            Priority,
            Channel,
            Maximum,
            GraceDays,
            Rating,
            Country,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
