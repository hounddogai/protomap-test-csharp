namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled adjustment summary is in its life cycle.</summary>
public enum ScheduledAdjustmentSummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a scheduled adjustment summary and its current status.</summary>
public sealed class ScheduledAdjustmentSummary(string code, string label, int limit, double rating, string checksum, ScheduledAdjustmentSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public double Rating { get; set; } = rating;
    public string Checksum { get; set; } = checksum;
    public ScheduledAdjustmentSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled adjustment summary in status proposed.</summary>
    public static ScheduledAdjustmentSummary NewScheduledAdjustmentSummary(string code, string label)
    {
        return new ScheduledAdjustmentSummary(code, label, 0, 0.0, "", ScheduledAdjustmentSummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAdjustmentSummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAdjustmentSummaryStatus next)
    {
        switch (Status)
        {
            case ScheduledAdjustmentSummaryStatus.Proposed:
                return next == ScheduledAdjustmentSummaryStatus.Approved || next == ScheduledAdjustmentSummaryStatus.Withdrawn;
            case ScheduledAdjustmentSummaryStatus.Approved:
                switch (next)
                {
                    case ScheduledAdjustmentSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScheduledAdjustmentSummaryStatus.Rejected:
                switch (next)
                {
                    case ScheduledAdjustmentSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledAdjustmentSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled adjustment summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled adjustment summary"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Rating + (double)Limit, 2);
    }

    public ScheduledAdjustmentSummary Copy()
    {
        return new ScheduledAdjustmentSummary(Code, Label, Limit, Rating, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
