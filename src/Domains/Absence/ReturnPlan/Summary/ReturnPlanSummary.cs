namespace Acme.Workforce.Absence.ReturnPlan.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan summary is in its life cycle.</summary>
public enum ReturnPlanSummaryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a return plan summary and its current status.</summary>
public sealed class ReturnPlanSummary(string code, string name, int graceDays, string channel, int endsAt, string externalRef, bool archived, int attempts, ReturnPlanSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public string Channel { get; set; } = channel;
    public int EndsAt { get; set; } = endsAt;
    public string ExternalRef { get; set; } = externalRef;
    public bool Archived { get; set; } = archived;
    public int Attempts { get; set; } = attempts;
    public ReturnPlanSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a return plan summary in status planned.</summary>
    public static ReturnPlanSummary NewReturnPlanSummary(string code, string name)
    {
        return new ReturnPlanSummary(code, name, 0, "", 0, "", false, 0, ReturnPlanSummaryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanSummaryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanSummaryStatus next)
    {
        switch (Status)
        {
            case ReturnPlanSummaryStatus.Planned:
                return next == ReturnPlanSummaryStatus.InProgress;
            case ReturnPlanSummaryStatus.Scheduled:
                return next == ReturnPlanSummaryStatus.Completed || next == ReturnPlanSummaryStatus.Cancelled;
            case ReturnPlanSummaryStatus.InProgress:
                switch (next)
                {
                    case ReturnPlanSummaryStatus.Completed:
                    case ReturnPlanSummaryStatus.Cancelled:
                        return true;
                }
                return false;
            case ReturnPlanSummaryStatus.Completed:
                return next == ReturnPlanSummaryStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ReturnPlanSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Attempts * (double)EndsAt, 2);
    }

    public ReturnPlanSummary Copy()
    {
        return new ReturnPlanSummary(
            Code,
            Name,
            GraceDays,
            Channel,
            EndsAt,
            ExternalRef,
            Archived,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
