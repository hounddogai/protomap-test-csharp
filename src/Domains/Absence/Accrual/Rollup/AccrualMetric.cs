namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual metric is in its life cycle.</summary>
public enum AccrualMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a accrual metric and its current status.</summary>
public sealed class AccrualMetric(string code, string label, string department, double weight, int graceDays, bool active, string timezone, AccrualMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public double Weight { get; set; } = weight;
    public int GraceDays { get; set; } = graceDays;
    public bool Active { get; set; } = active;
    public string Timezone { get; set; } = timezone;
    public AccrualMetricStatus Status { get; set; } = status;

    /// <summary>Starts a accrual metric in status planned.</summary>
    public static AccrualMetric NewAccrualMetric(string code, string label)
    {
        return new AccrualMetric(code, label, "", 0.0, 0, false, "", AccrualMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualMetricStatus next)
    {
        switch (Status)
        {
            case AccrualMetricStatus.Planned:
                switch (next)
                {
                    case AccrualMetricStatus.Scheduled:
                    case AccrualMetricStatus.Completed:
                        return true;
                }
                return false;
            case AccrualMetricStatus.Scheduled:
                return next == AccrualMetricStatus.Completed;
            case AccrualMetricStatus.InProgress:
                switch (next)
                {
                    case AccrualMetricStatus.Completed:
                        return true;
                }
                return false;
            case AccrualMetricStatus.Completed:
                return next == AccrualMetricStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AccrualMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)GraceDays + Weight, 2);
    }

    public AccrualMetric Copy()
    {
        return new AccrualMetric(Code, Label, Department, Weight, GraceDays, Active, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
