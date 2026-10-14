namespace Acme.Workforce.Payrollops.Garnishment.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment summary is in its life cycle.</summary>
public enum GarnishmentSummaryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a garnishment summary and its current status.</summary>
public sealed class GarnishmentSummary(string code, string label, double rating, string team, double cost, GarnishmentSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public string Team { get; set; } = team;
    public double Cost { get; set; } = cost;
    public GarnishmentSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment summary in status draft.</summary>
    public static GarnishmentSummary NewGarnishmentSummary(string code, string label)
    {
        return new GarnishmentSummary(code, label, 0.0, "", 0.0, GarnishmentSummaryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentSummaryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentSummaryStatus next)
    {
        switch (Status)
        {
            case GarnishmentSummaryStatus.Draft:
                return next == GarnishmentSummaryStatus.Pending || next == GarnishmentSummaryStatus.Suspended;
            case GarnishmentSummaryStatus.Pending:
                switch (next)
                {
                    case GarnishmentSummaryStatus.Suspended:
                    case GarnishmentSummaryStatus.Closed:
                        return true;
                }
                return false;
            case GarnishmentSummaryStatus.Active:
                switch (next)
                {
                    case GarnishmentSummaryStatus.Suspended:
                    case GarnishmentSummaryStatus.Closed:
                        return true;
                }
                return false;
            case GarnishmentSummaryStatus.Suspended:
                return next == GarnishmentSummaryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(GarnishmentSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Rating * Cost, 2);
    }

    public GarnishmentSummary Copy()
    {
        return new GarnishmentSummary(Code, Label, Rating, Team, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
