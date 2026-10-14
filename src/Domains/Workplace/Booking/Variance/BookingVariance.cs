namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking variance is in its life cycle.</summary>
public enum BookingVarianceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a booking variance and its current status.</summary>
public sealed class BookingVariance(string code, string title, double cost, int tenureMonths, double hours, string grade, int revision, double threshold, string timezone, string reviewer, BookingVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Hours { get; set; } = hours;
    public string Grade { get; set; } = grade;
    public int Revision { get; set; } = revision;
    public double Threshold { get; set; } = threshold;
    public string Timezone { get; set; } = timezone;
    public string Reviewer { get; set; } = reviewer;
    public BookingVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a booking variance in status new.</summary>
    public static BookingVariance NewBookingVariance(string code, string title)
    {
        return new BookingVariance(code, title, 0.0, 0, 0.0, "", 0, 0.0, "", "", BookingVarianceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingVarianceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingVarianceStatus next)
    {
        switch (Status)
        {
            case BookingVarianceStatus.New:
                switch (next)
                {
                    case BookingVarianceStatus.Queued:
                    case BookingVarianceStatus.Failed:
                        return true;
                }
                return false;
            case BookingVarianceStatus.Queued:
                return next == BookingVarianceStatus.Failed;
            case BookingVarianceStatus.Running:
                return next == BookingVarianceStatus.Done || next == BookingVarianceStatus.Failed;
            case BookingVarianceStatus.Done:
                switch (next)
                {
                    case BookingVarianceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedVolume()
    {
        return Math.Round(Threshold + Cost, 2);
    }

    public BookingVariance Copy()
    {
        return new BookingVariance(
            Code,
            Title,
            Cost,
            TenureMonths,
            Hours,
            Grade,
            Revision,
            Threshold,
            Timezone,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
