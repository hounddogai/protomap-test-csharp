namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient nomination forecast is in its life cycle.</summary>
public enum LenientNominationForecastStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A lenient nomination forecast. Its code is unique within a tenant.</summary>
public sealed class LenientNominationForecast(string code, string title, string unit, double ceiling, double maximum, int createdAt, int tenureMonths, string region, string category, LenientNominationForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public double Ceiling { get; set; } = ceiling;
    public double Maximum { get; set; } = maximum;
    public int CreatedAt { get; set; } = createdAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Region { get; set; } = region;
    public string Category { get; set; } = category;
    public LenientNominationForecastStatus Status { get; set; } = status;

    /// <summary>Starts a lenient nomination forecast in status open.</summary>
    public static LenientNominationForecast NewLenientNominationForecast(string code, string title)
    {
        return new LenientNominationForecast(
            code,
            title,
            "",
            0.0,
            0.0,
            0,
            0,
            "",
            "",
            LenientNominationForecastStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientNominationForecastStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientNominationForecastStatus next)
    {
        switch (Status)
        {
            case LenientNominationForecastStatus.Open:
                return next == LenientNominationForecastStatus.Archived;
            case LenientNominationForecastStatus.InReview:
                switch (next)
                {
                    case LenientNominationForecastStatus.Reopened:
                        return true;
                }
                return false;
            case LenientNominationForecastStatus.Resolved:
                return next == LenientNominationForecastStatus.Reopened || next == LenientNominationForecastStatus.Archived;
            case LenientNominationForecastStatus.Reopened:
                return next == LenientNominationForecastStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LenientNominationForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient nomination forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetExposure()
    {
        return Math.Round((double)TenureMonths - Ceiling, 2);
    }

    public LenientNominationForecast Copy()
    {
        return new LenientNominationForecast(
            Code,
            Title,
            Unit,
            Ceiling,
            Maximum,
            CreatedAt,
            TenureMonths,
            Region,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
