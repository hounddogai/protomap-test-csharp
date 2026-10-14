namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary equity grant summary is in its life cycle.</summary>
public enum PrimaryEquityGrantSummaryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A primary equity grant summary. Its code is unique within a tenant.</summary>
public sealed class PrimaryEquityGrantSummary(string code, string title, string channel, double budget, double score, string category, int tenureMonths, PrimaryEquityGrantSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public double Score { get; set; } = score;
    public string Category { get; set; } = category;
    public int TenureMonths { get; set; } = tenureMonths;
    public PrimaryEquityGrantSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a primary equity grant summary in status draft.</summary>
    public static PrimaryEquityGrantSummary NewPrimaryEquityGrantSummary(string code, string title)
    {
        return new PrimaryEquityGrantSummary(
            code,
            title,
            "",
            0.0,
            0.0,
            "",
            0,
            PrimaryEquityGrantSummaryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryEquityGrantSummaryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryEquityGrantSummaryStatus next)
    {
        switch (Status)
        {
            case PrimaryEquityGrantSummaryStatus.Draft:
                switch (next)
                {
                    case PrimaryEquityGrantSummaryStatus.Active:
                    case PrimaryEquityGrantSummaryStatus.Suspended:
                        return true;
                }
                return false;
            case PrimaryEquityGrantSummaryStatus.Pending:
                switch (next)
                {
                    case PrimaryEquityGrantSummaryStatus.Active:
                    case PrimaryEquityGrantSummaryStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryEquityGrantSummaryStatus.Active:
                return next == PrimaryEquityGrantSummaryStatus.Suspended;
            case PrimaryEquityGrantSummaryStatus.Suspended:
                return next == PrimaryEquityGrantSummaryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PrimaryEquityGrantSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary equity grant summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary equity grant summary"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)TenureMonths + Budget, 2);
    }

    public PrimaryEquityGrantSummary Copy()
    {
        return new PrimaryEquityGrantSummary(Code, Title, Channel, Budget, Score, Category, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
