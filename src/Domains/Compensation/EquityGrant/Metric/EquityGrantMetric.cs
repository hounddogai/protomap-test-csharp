namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant metric is in its life cycle.</summary>
public enum EquityGrantMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A equity grant metric. Its code is unique within a tenant.</summary>
public sealed class EquityGrantMetric(string code, string label, string managerId, int expiresAt, string department, double discount, string externalRef, int priority, EquityGrantMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Department { get; set; } = department;
    public double Discount { get; set; } = discount;
    public string ExternalRef { get; set; } = externalRef;
    public int Priority { get; set; } = priority;
    public EquityGrantMetricStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant metric in status planned.</summary>
    public static EquityGrantMetric NewEquityGrantMetric(string code, string label)
    {
        return new EquityGrantMetric(code, label, "", 0, "", 0.0, "", 0, EquityGrantMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantMetricStatus next)
    {
        switch (Status)
        {
            case EquityGrantMetricStatus.Planned:
                return next == EquityGrantMetricStatus.InProgress;
            case EquityGrantMetricStatus.Scheduled:
                return next == EquityGrantMetricStatus.InProgress || next == EquityGrantMetricStatus.Completed;
            case EquityGrantMetricStatus.InProgress:
                switch (next)
                {
                    case EquityGrantMetricStatus.Completed:
                        return true;
                }
                return false;
            case EquityGrantMetricStatus.Completed:
                return next == EquityGrantMetricStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(EquityGrantMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"equity grant metric"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Discount - (double)Priority, 2);
    }

    public EquityGrantMetric Copy()
    {
        return new EquityGrantMetric(
            Code,
            Label,
            ManagerId,
            ExpiresAt,
            Department,
            Discount,
            ExternalRef,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
