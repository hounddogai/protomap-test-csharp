namespace Acme.Workforce.Absence.ReturnPlan.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan metric is in its life cycle.</summary>
public enum ReturnPlanMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A return plan metric as the return plan team keeps it.</summary>
public sealed class ReturnPlanMetric(string code, string label, string location, double amount, string segment, ReturnPlanMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public double Amount { get; set; } = amount;
    public string Segment { get; set; } = segment;
    public ReturnPlanMetricStatus Status { get; set; } = status;

    /// <summary>Starts a return plan metric in status open.</summary>
    public static ReturnPlanMetric NewReturnPlanMetric(string code, string label)
    {
        return new ReturnPlanMetric(code, label, "", 0.0, "", ReturnPlanMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanMetricStatus next)
    {
        switch (Status)
        {
            case ReturnPlanMetricStatus.Open:
                switch (next)
                {
                    case ReturnPlanMetricStatus.Reopened:
                    case ReturnPlanMetricStatus.Archived:
                        return true;
                }
                return false;
            case ReturnPlanMetricStatus.InReview:
                return next == ReturnPlanMetricStatus.Archived;
            case ReturnPlanMetricStatus.Resolved:
                switch (next)
                {
                    case ReturnPlanMetricStatus.Reopened:
                    case ReturnPlanMetricStatus.Archived:
                        return true;
                }
                return false;
            case ReturnPlanMetricStatus.Reopened:
                switch (next)
                {
                    case ReturnPlanMetricStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public ReturnPlanMetric Copy()
    {
        return new ReturnPlanMetric(Code, Label, Location, Amount, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
