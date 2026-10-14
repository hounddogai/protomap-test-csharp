namespace Acme.Workforce.Contractors.Rate.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict rate forecast is in its life cycle.</summary>
public enum StrictRateForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a strict rate forecast and its current status.</summary>
public sealed class StrictRateForecast(string code, string title, double maximum, string unit, double rating, string externalRef, double threshold, int startsAt, string badge, double hours, string segment, StrictRateForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public string Unit { get; set; } = unit;
    public double Rating { get; set; } = rating;
    public string ExternalRef { get; set; } = externalRef;
    public double Threshold { get; set; } = threshold;
    public int StartsAt { get; set; } = startsAt;
    public string Badge { get; set; } = badge;
    public double Hours { get; set; } = hours;
    public string Segment { get; set; } = segment;
    public StrictRateForecastStatus Status { get; set; } = status;

    /// <summary>Starts a strict rate forecast in status proposed.</summary>
    public static StrictRateForecast NewStrictRateForecast(string code, string title)
    {
        return new StrictRateForecast(
            code,
            title,
            0.0,
            "",
            0.0,
            "",
            0.0,
            0,
            "",
            0.0,
            "",
            StrictRateForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictRateForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictRateForecastStatus next)
    {
        switch (Status)
        {
            case StrictRateForecastStatus.Proposed:
                return next == StrictRateForecastStatus.Rejected;
            case StrictRateForecastStatus.Approved:
                return next == StrictRateForecastStatus.Withdrawn;
            case StrictRateForecastStatus.Rejected:
                switch (next)
                {
                    case StrictRateForecastStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictRateForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict rate forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Threshold * Rating, 2);
    }

    public StrictRateForecast Copy()
    {
        return new StrictRateForecast(
            Code,
            Title,
            Maximum,
            Unit,
            Rating,
            ExternalRef,
            Threshold,
            StartsAt,
            Badge,
            Hours,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
