namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal adjustment summary is in its life cycle.</summary>
public enum SeasonalAdjustmentSummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One seasonal adjustment summary, with what the planners need to act on it.</summary>
public sealed class SeasonalAdjustmentSummary(string code, string title, double maximum, int version, string category, double hours, double rate, string team, bool approved, SeasonalAdjustmentSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public int Version { get; set; } = version;
    public string Category { get; set; } = category;
    public double Hours { get; set; } = hours;
    public double Rate { get; set; } = rate;
    public string Team { get; set; } = team;
    public bool Approved { get; set; } = approved;
    public SeasonalAdjustmentSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal adjustment summary in status proposed.</summary>
    public static SeasonalAdjustmentSummary NewSeasonalAdjustmentSummary(string code, string title)
    {
        return new SeasonalAdjustmentSummary(
            code,
            title,
            0.0,
            0,
            "",
            0.0,
            0.0,
            "",
            false,
            SeasonalAdjustmentSummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAdjustmentSummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAdjustmentSummaryStatus next)
    {
        switch (Status)
        {
            case SeasonalAdjustmentSummaryStatus.Proposed:
                return next == SeasonalAdjustmentSummaryStatus.Approved;
            case SeasonalAdjustmentSummaryStatus.Approved:
                switch (next)
                {
                    case SeasonalAdjustmentSummaryStatus.Rejected:
                    case SeasonalAdjustmentSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
            case SeasonalAdjustmentSummaryStatus.Rejected:
                switch (next)
                {
                    case SeasonalAdjustmentSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalAdjustmentSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal adjustment summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Version - Rate, 2);
    }

    public SeasonalAdjustmentSummary Copy()
    {
        return new SeasonalAdjustmentSummary(
            Code,
            Title,
            Maximum,
            Version,
            Category,
            Hours,
            Rate,
            Team,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
