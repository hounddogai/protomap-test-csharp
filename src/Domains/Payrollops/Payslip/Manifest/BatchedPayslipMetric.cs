namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched payslip metric is in its life cycle.</summary>
public enum BatchedPayslipMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One batched payslip metric, with what the planners need to act on it.</summary>
public sealed class BatchedPayslipMetric(string code, string title, string unit, double threshold, double floorValue, BatchedPayslipMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public double Threshold { get; set; } = threshold;
    public double FloorValue { get; set; } = floorValue;
    public BatchedPayslipMetricStatus Status { get; set; } = status;

    /// <summary>Starts a batched payslip metric in status planned.</summary>
    public static BatchedPayslipMetric NewBatchedPayslipMetric(string code, string title)
    {
        return new BatchedPayslipMetric(code, title, "", 0.0, 0.0, BatchedPayslipMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPayslipMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPayslipMetricStatus next)
    {
        switch (Status)
        {
            case BatchedPayslipMetricStatus.Planned:
                return next == BatchedPayslipMetricStatus.InProgress;
            case BatchedPayslipMetricStatus.Scheduled:
                return next == BatchedPayslipMetricStatus.Cancelled;
            case BatchedPayslipMetricStatus.InProgress:
                switch (next)
                {
                    case BatchedPayslipMetricStatus.Completed:
                    case BatchedPayslipMetricStatus.Cancelled:
                        return true;
                }
                return false;
            case BatchedPayslipMetricStatus.Completed:
                switch (next)
                {
                    case BatchedPayslipMetricStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedPayslipMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched payslip metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Threshold - FloorValue, 2);
    }

    public BatchedPayslipMetric Copy()
    {
        return new BatchedPayslipMetric(Code, Title, Unit, Threshold, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
