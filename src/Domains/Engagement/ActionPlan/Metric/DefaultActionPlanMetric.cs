namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default action plan metric is in its life cycle.</summary>
public enum DefaultActionPlanMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a default action plan metric and its current status.</summary>
public sealed class DefaultActionPlanMetric(string code, string title, string grade, string unit, string employeeId, string locale, int sequence, DefaultActionPlanMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public string Unit { get; set; } = unit;
    public string EmployeeId { get; set; } = employeeId;
    public string Locale { get; set; } = locale;
    public int Sequence { get; set; } = sequence;
    public DefaultActionPlanMetricStatus Status { get; set; } = status;

    /// <summary>Starts a default action plan metric in status proposed.</summary>
    public static DefaultActionPlanMetric NewDefaultActionPlanMetric(string code, string title)
    {
        return new DefaultActionPlanMetric(code, title, "", "", "", "", 0, DefaultActionPlanMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultActionPlanMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultActionPlanMetricStatus next)
    {
        switch (Status)
        {
            case DefaultActionPlanMetricStatus.Proposed:
                switch (next)
                {
                    case DefaultActionPlanMetricStatus.Approved:
                    case DefaultActionPlanMetricStatus.Rejected:
                        return true;
                }
                return false;
            case DefaultActionPlanMetricStatus.Approved:
                return next == DefaultActionPlanMetricStatus.Withdrawn;
            case DefaultActionPlanMetricStatus.Rejected:
                return next == DefaultActionPlanMetricStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DefaultActionPlanMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default action plan metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public DefaultActionPlanMetric Copy()
    {
        return new DefaultActionPlanMetric(Code, Title, Grade, Unit, EmployeeId, Locale, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
