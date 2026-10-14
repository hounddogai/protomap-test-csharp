namespace Acme.Workforce.Workplace.Occupancy.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict occupancy summary is in its life cycle.</summary>
public enum StrictOccupancySummaryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a strict occupancy summary and its current status.</summary>
public sealed class StrictOccupancySummary(string code, string title, int expiresAt, double margin, double cost, int priority, double maximum, double discount, StrictOccupancySummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Margin { get; set; } = margin;
    public double Cost { get; set; } = cost;
    public int Priority { get; set; } = priority;
    public double Maximum { get; set; } = maximum;
    public double Discount { get; set; } = discount;
    public StrictOccupancySummaryStatus Status { get; set; } = status;

    /// <summary>Starts a strict occupancy summary in status open.</summary>
    public static StrictOccupancySummary NewStrictOccupancySummary(string code, string title)
    {
        return new StrictOccupancySummary(code, title, 0, 0.0, 0.0, 0, 0.0, 0.0, StrictOccupancySummaryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictOccupancySummaryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictOccupancySummaryStatus next)
    {
        switch (Status)
        {
            case StrictOccupancySummaryStatus.Open:
                return next == StrictOccupancySummaryStatus.InReview || next == StrictOccupancySummaryStatus.Resolved;
            case StrictOccupancySummaryStatus.InReview:
                return next == StrictOccupancySummaryStatus.Archived;
            case StrictOccupancySummaryStatus.Resolved:
                switch (next)
                {
                    case StrictOccupancySummaryStatus.Reopened:
                        return true;
                }
                return false;
            case StrictOccupancySummaryStatus.Reopened:
                return next == StrictOccupancySummaryStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictOccupancySummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict occupancy summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"strict occupancy summary"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Margin * (double)ExpiresAt, 2);
    }

    public StrictOccupancySummary Copy()
    {
        return new StrictOccupancySummary(Code, Title, ExpiresAt, Margin, Cost, Priority, Maximum, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
