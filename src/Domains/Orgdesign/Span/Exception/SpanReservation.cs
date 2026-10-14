namespace Acme.Workforce.Orgdesign.Span.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span reservation is in its life cycle.</summary>
public enum SpanReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A span reservation as the span team keeps it.</summary>
public sealed class SpanReservation(string code, string title, string description, string badge, string region, int durationMinutes, string category, int sequence, List<string> tags, int windowDays, double ceiling, SpanReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public string Badge { get; set; } = badge;
    public string Region { get; set; } = region;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Category { get; set; } = category;
    public int Sequence { get; set; } = sequence;
    public List<string> Tags { get; set; } = tags;
    public int WindowDays { get; set; } = windowDays;
    public double Ceiling { get; set; } = ceiling;
    public SpanReservationStatus Status { get; set; } = status;

    /// <summary>Starts a span reservation in status requested.</summary>
    public static SpanReservation NewSpanReservation(string code, string title)
    {
        return new SpanReservation(
            code,
            title,
            "",
            "",
            "",
            0,
            "",
            0,
            new List<string>(),
            0,
            0.0,
            SpanReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanReservationStatus next)
    {
        switch (Status)
        {
            case SpanReservationStatus.Requested:
                switch (next)
                {
                    case SpanReservationStatus.Granted:
                        return true;
                }
                return false;
            case SpanReservationStatus.Granted:
                return next == SpanReservationStatus.Expired;
            case SpanReservationStatus.Revoked:
                return next == SpanReservationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SpanReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Sequence - (double)DurationMinutes, 2);
    }

    public SpanReservation Copy()
    {
        return new SpanReservation(
            Code,
            Title,
            Description,
            Badge,
            Region,
            DurationMinutes,
            Category,
            Sequence,
            Tags,
            WindowDays,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
