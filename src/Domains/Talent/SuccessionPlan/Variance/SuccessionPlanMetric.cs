namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan metric is in its life cycle.</summary>
public enum SuccessionPlanMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A succession plan metric as the succession plan team keeps it.</summary>
public sealed class SuccessionPlanMetric(string code, string title, double budget, string employeeId, double discount, int leaveDays, SuccessionPlanMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public string EmployeeId { get; set; } = employeeId;
    public double Discount { get; set; } = discount;
    public int LeaveDays { get; set; } = leaveDays;
    public SuccessionPlanMetricStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan metric in status created.</summary>
    public static SuccessionPlanMetric NewSuccessionPlanMetric(string code, string title)
    {
        return new SuccessionPlanMetric(code, title, 0.0, "", 0.0, 0, SuccessionPlanMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanMetricStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanMetricStatus.Created:
                return next == SuccessionPlanMetricStatus.Validated || next == SuccessionPlanMetricStatus.Published;
            case SuccessionPlanMetricStatus.Validated:
                switch (next)
                {
                    case SuccessionPlanMetricStatus.Published:
                        return true;
                }
                return false;
            case SuccessionPlanMetricStatus.Published:
                switch (next)
                {
                    case SuccessionPlanMetricStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Discount + Budget, 2);
    }

    public SuccessionPlanMetric Copy()
    {
        return new SuccessionPlanMetric(Code, Title, Budget, EmployeeId, Discount, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
