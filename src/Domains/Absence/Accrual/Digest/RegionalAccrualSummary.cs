namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional accrual summary is in its life cycle.</summary>
public enum RegionalAccrualSummaryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One regional accrual summary, with what the planners need to act on it.</summary>
public sealed class RegionalAccrualSummary(string code, string title, int batchSize, string category, double amount, int graceDays, int windowDays, int priority, RegionalAccrualSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public string Category { get; set; } = category;
    public double Amount { get; set; } = amount;
    public int GraceDays { get; set; } = graceDays;
    public int WindowDays { get; set; } = windowDays;
    public int Priority { get; set; } = priority;
    public RegionalAccrualSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a regional accrual summary in status open.</summary>
    public static RegionalAccrualSummary NewRegionalAccrualSummary(string code, string title)
    {
        return new RegionalAccrualSummary(code, title, 0, "", 0.0, 0, 0, 0, RegionalAccrualSummaryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAccrualSummaryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAccrualSummaryStatus next)
    {
        switch (Status)
        {
            case RegionalAccrualSummaryStatus.Open:
                return next == RegionalAccrualSummaryStatus.Resolved;
            case RegionalAccrualSummaryStatus.InReview:
                switch (next)
                {
                    case RegionalAccrualSummaryStatus.Resolved:
                    case RegionalAccrualSummaryStatus.Archived:
                        return true;
                }
                return false;
            case RegionalAccrualSummaryStatus.Resolved:
                return next == RegionalAccrualSummaryStatus.Reopened || next == RegionalAccrualSummaryStatus.Archived;
            case RegionalAccrualSummaryStatus.Reopened:
                switch (next)
                {
                    case RegionalAccrualSummaryStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalAccrualSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional accrual summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Priority + (double)WindowDays, 2);
    }

    public RegionalAccrualSummary Copy()
    {
        return new RegionalAccrualSummary(
            Code,
            Title,
            BatchSize,
            Category,
            Amount,
            GraceDays,
            WindowDays,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
