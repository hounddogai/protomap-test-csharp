namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span score is in its life cycle.</summary>
public enum SpanScoreStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One span score, with what the planners need to act on it.</summary>
public sealed class SpanScore(string code, string title, string location, int startsAt, int graceDays, string checksum, SpanScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public int StartsAt { get; set; } = startsAt;
    public int GraceDays { get; set; } = graceDays;
    public string Checksum { get; set; } = checksum;
    public SpanScoreStatus Status { get; set; } = status;

    /// <summary>Starts a span score in status planned.</summary>
    public static SpanScore NewSpanScore(string code, string title)
    {
        return new SpanScore(code, title, "", 0, 0, "", SpanScoreStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanScoreStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanScoreStatus next)
    {
        switch (Status)
        {
            case SpanScoreStatus.Planned:
                switch (next)
                {
                    case SpanScoreStatus.InProgress:
                        return true;
                }
                return false;
            case SpanScoreStatus.Scheduled:
                return next == SpanScoreStatus.InProgress || next == SpanScoreStatus.Cancelled;
            case SpanScoreStatus.InProgress:
                return next == SpanScoreStatus.Completed || next == SpanScoreStatus.Cancelled;
            case SpanScoreStatus.Completed:
                return next == SpanScoreStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SpanScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"span score"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)StartsAt * (double)GraceDays, 2);
    }

    public SpanScore Copy()
    {
        return new SpanScore(Code, Title, Location, StartsAt, GraceDays, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
