namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary engagement summary is in its life cycle.</summary>
public enum PrimaryEngagementSummaryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A primary engagement summary as the engagement team keeps it.</summary>
public sealed class PrimaryEngagementSummary(string code, string title, int windowDays, int graceDays, double score, PrimaryEngagementSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public int GraceDays { get; set; } = graceDays;
    public double Score { get; set; } = score;
    public PrimaryEngagementSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a primary engagement summary in status created.</summary>
    public static PrimaryEngagementSummary NewPrimaryEngagementSummary(string code, string title)
    {
        return new PrimaryEngagementSummary(code, title, 0, 0, 0.0, PrimaryEngagementSummaryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryEngagementSummaryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryEngagementSummaryStatus next)
    {
        switch (Status)
        {
            case PrimaryEngagementSummaryStatus.Created:
                return next == PrimaryEngagementSummaryStatus.Validated;
            case PrimaryEngagementSummaryStatus.Validated:
                return next == PrimaryEngagementSummaryStatus.Published;
            case PrimaryEngagementSummaryStatus.Published:
                return next == PrimaryEngagementSummaryStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PrimaryEngagementSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary engagement summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Score - (double)WindowDays, 2);
    }

    public PrimaryEngagementSummary Copy()
    {
        return new PrimaryEngagementSummary(Code, Title, WindowDays, GraceDays, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
