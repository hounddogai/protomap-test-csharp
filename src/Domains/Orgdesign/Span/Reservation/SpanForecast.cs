namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span forecast is in its life cycle.</summary>
public enum SpanForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A span forecast as the span team keeps it.</summary>
public sealed class SpanForecast(string code, string label, string category, string region, int endsAt, bool approved, double maximum, string location, int expiresAt, SpanForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public string Region { get; set; } = region;
    public int EndsAt { get; set; } = endsAt;
    public bool Approved { get; set; } = approved;
    public double Maximum { get; set; } = maximum;
    public string Location { get; set; } = location;
    public int ExpiresAt { get; set; } = expiresAt;
    public SpanForecastStatus Status { get; set; } = status;

    /// <summary>Starts a span forecast in status draft.</summary>
    public static SpanForecast NewSpanForecast(string code, string label)
    {
        return new SpanForecast(code, label, "", "", 0, false, 0.0, "", 0, SpanForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanForecastStatus next)
    {
        switch (Status)
        {
            case SpanForecastStatus.Draft:
                return next == SpanForecastStatus.Suspended;
            case SpanForecastStatus.Pending:
                switch (next)
                {
                    case SpanForecastStatus.Active:
                    case SpanForecastStatus.Closed:
                        return true;
                }
                return false;
            case SpanForecastStatus.Active:
                switch (next)
                {
                    case SpanForecastStatus.Closed:
                        return true;
                }
                return false;
            case SpanForecastStatus.Suspended:
                return next == SpanForecastStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SpanForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"span forecast"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Maximum - (double)ExpiresAt, 2);
    }

    public SpanForecast Copy()
    {
        return new SpanForecast(
            Code,
            Label,
            Category,
            Region,
            EndsAt,
            Approved,
            Maximum,
            Location,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
