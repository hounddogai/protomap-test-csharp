namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor summary is in its life cycle.</summary>
public enum VisitorSummaryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A visitor summary as the visitor team keeps it.</summary>
public sealed class VisitorSummary(string code, string title, double ceiling, int endsAt, string source, double fte, double maximum, int quantity, VisitorSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public int EndsAt { get; set; } = endsAt;
    public string Source { get; set; } = source;
    public double Fte { get; set; } = fte;
    public double Maximum { get; set; } = maximum;
    public int Quantity { get; set; } = quantity;
    public VisitorSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a visitor summary in status draft.</summary>
    public static VisitorSummary NewVisitorSummary(string code, string title)
    {
        return new VisitorSummary(code, title, 0.0, 0, "", 0.0, 0.0, 0, VisitorSummaryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorSummaryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorSummaryStatus next)
    {
        switch (Status)
        {
            case VisitorSummaryStatus.Draft:
                return next == VisitorSummaryStatus.Active || next == VisitorSummaryStatus.Closed;
            case VisitorSummaryStatus.Pending:
                return next == VisitorSummaryStatus.Active || next == VisitorSummaryStatus.Suspended;
            case VisitorSummaryStatus.Active:
                switch (next)
                {
                    case VisitorSummaryStatus.Suspended:
                        return true;
                }
                return false;
            case VisitorSummaryStatus.Suspended:
                return next == VisitorSummaryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(VisitorSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Fte * Maximum, 2);
    }

    public VisitorSummary Copy()
    {
        return new VisitorSummary(Code, Title, Ceiling, EndsAt, Source, Fte, Maximum, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
