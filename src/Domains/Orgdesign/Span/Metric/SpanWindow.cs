namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span window is in its life cycle.</summary>
public enum SpanWindowStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a span window and its current status.</summary>
public sealed class SpanWindow(string code, string title, bool approved, string summary, int windowDays, int retries, double budget, string country, SpanWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public string Summary { get; set; } = summary;
    public int WindowDays { get; set; } = windowDays;
    public int Retries { get; set; } = retries;
    public double Budget { get; set; } = budget;
    public string Country { get; set; } = country;
    public SpanWindowStatus Status { get; set; } = status;

    /// <summary>Starts a span window in status draft.</summary>
    public static SpanWindow NewSpanWindow(string code, string title)
    {
        return new SpanWindow(code, title, false, "", 0, 0, 0.0, "", SpanWindowStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanWindowStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanWindowStatus next)
    {
        switch (Status)
        {
            case SpanWindowStatus.Draft:
                return next == SpanWindowStatus.Active || next == SpanWindowStatus.Closed;
            case SpanWindowStatus.Pending:
                switch (next)
                {
                    case SpanWindowStatus.Active:
                    case SpanWindowStatus.Suspended:
                        return true;
                }
                return false;
            case SpanWindowStatus.Active:
                switch (next)
                {
                    case SpanWindowStatus.Closed:
                        return true;
                }
                return false;
            case SpanWindowStatus.Suspended:
                switch (next)
                {
                    case SpanWindowStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)WindowDays + (double)Retries, 2);
    }

    public SpanWindow Copy()
    {
        return new SpanWindow(Code, Title, Approved, Summary, WindowDays, Retries, Budget, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
