namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal holiday exception is in its life cycle.</summary>
public enum SeasonalHolidayExceptionStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A seasonal holiday exception. Its code is unique within a tenant.</summary>
public sealed class SeasonalHolidayException(string code, string name, string summary, string locale, string ownerId, double discount, double floorValue, int retries, string grade, int expiresAt, SeasonalHolidayExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public string Locale { get; set; } = locale;
    public string OwnerId { get; set; } = ownerId;
    public double Discount { get; set; } = discount;
    public double FloorValue { get; set; } = floorValue;
    public int Retries { get; set; } = retries;
    public string Grade { get; set; } = grade;
    public int ExpiresAt { get; set; } = expiresAt;
    public SeasonalHolidayExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal holiday exception in status open.</summary>
    public static SeasonalHolidayException NewSeasonalHolidayException(string code, string name)
    {
        return new SeasonalHolidayException(
            code,
            name,
            "",
            "",
            "",
            0.0,
            0.0,
            0,
            "",
            0,
            SeasonalHolidayExceptionStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalHolidayExceptionStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalHolidayExceptionStatus next)
    {
        switch (Status)
        {
            case SeasonalHolidayExceptionStatus.Open:
                return next == SeasonalHolidayExceptionStatus.Resolved || next == SeasonalHolidayExceptionStatus.Reopened;
            case SeasonalHolidayExceptionStatus.InReview:
                return next == SeasonalHolidayExceptionStatus.Reopened;
            case SeasonalHolidayExceptionStatus.Resolved:
                switch (next)
                {
                    case SeasonalHolidayExceptionStatus.Reopened:
                    case SeasonalHolidayExceptionStatus.Archived:
                        return true;
                }
                return false;
            case SeasonalHolidayExceptionStatus.Reopened:
                return next == SeasonalHolidayExceptionStatus.Archived;
        }
        return false;
    }

    public void MoveTo(SeasonalHolidayExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal holiday exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)ExpiresAt + (double)Retries, 2);
    }

    public SeasonalHolidayException Copy()
    {
        return new SeasonalHolidayException(
            Code,
            Name,
            Summary,
            Locale,
            OwnerId,
            Discount,
            FloorValue,
            Retries,
            Grade,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
