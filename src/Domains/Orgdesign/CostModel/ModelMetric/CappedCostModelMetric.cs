namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped cost model metric is in its life cycle.</summary>
public enum CappedCostModelMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A capped cost model metric. Its code is unique within a tenant.</summary>
public sealed class CappedCostModelMetric(string code, string label, int version, double margin, string phone, int priority, bool approved, double rating, string badge, int leaveDays, double threshold, CappedCostModelMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public double Margin { get; set; } = margin;
    public string Phone { get; set; } = phone;
    public int Priority { get; set; } = priority;
    public bool Approved { get; set; } = approved;
    public double Rating { get; set; } = rating;
    public string Badge { get; set; } = badge;
    public int LeaveDays { get; set; } = leaveDays;
    public double Threshold { get; set; } = threshold;
    public CappedCostModelMetricStatus Status { get; set; } = status;

    /// <summary>Starts a capped cost model metric in status planned.</summary>
    public static CappedCostModelMetric NewCappedCostModelMetric(string code, string label)
    {
        return new CappedCostModelMetric(
            code,
            label,
            0,
            0.0,
            "",
            0,
            false,
            0.0,
            "",
            0,
            0.0,
            CappedCostModelMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedCostModelMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedCostModelMetricStatus next)
    {
        switch (Status)
        {
            case CappedCostModelMetricStatus.Planned:
                switch (next)
                {
                    case CappedCostModelMetricStatus.Completed:
                    case CappedCostModelMetricStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedCostModelMetricStatus.Scheduled:
                return next == CappedCostModelMetricStatus.InProgress || next == CappedCostModelMetricStatus.Completed;
            case CappedCostModelMetricStatus.InProgress:
                switch (next)
                {
                    case CappedCostModelMetricStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedCostModelMetricStatus.Completed:
                return next == CappedCostModelMetricStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CappedCostModelMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped cost model metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Version - (double)Priority, 2);
    }

    public CappedCostModelMetric Copy()
    {
        return new CappedCostModelMetric(
            Code,
            Label,
            Version,
            Margin,
            Phone,
            Priority,
            Approved,
            Rating,
            Badge,
            LeaveDays,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
