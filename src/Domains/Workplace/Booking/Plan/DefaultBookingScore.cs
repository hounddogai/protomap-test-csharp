namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default booking score is in its life cycle.</summary>
public enum DefaultBookingScoreStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A default booking score as the booking team keeps it.</summary>
public sealed class DefaultBookingScore(string code, string name, int priority, double ceiling, int batchSize, int tenureMonths, string locale, DefaultBookingScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public double Ceiling { get; set; } = ceiling;
    public int BatchSize { get; set; } = batchSize;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Locale { get; set; } = locale;
    public DefaultBookingScoreStatus Status { get; set; } = status;

    /// <summary>Starts a default booking score in status created.</summary>
    public static DefaultBookingScore NewDefaultBookingScore(string code, string name)
    {
        return new DefaultBookingScore(code, name, 0, 0.0, 0, 0, "", DefaultBookingScoreStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultBookingScoreStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultBookingScoreStatus next)
    {
        switch (Status)
        {
            case DefaultBookingScoreStatus.Created:
                return next == DefaultBookingScoreStatus.Published;
            case DefaultBookingScoreStatus.Validated:
                return next == DefaultBookingScoreStatus.Published;
            case DefaultBookingScoreStatus.Published:
                switch (next)
                {
                    case DefaultBookingScoreStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultBookingScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default booking score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetValue()
    {
        return Math.Round((double)BatchSize * (double)Priority, 2);
    }

    public DefaultBookingScore Copy()
    {
        return new DefaultBookingScore(Code, Name, Priority, Ceiling, BatchSize, TenureMonths, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
