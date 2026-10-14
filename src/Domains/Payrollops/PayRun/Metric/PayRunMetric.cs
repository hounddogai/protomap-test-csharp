namespace Acme.Workforce.Payrollops.PayRun.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run metric is in its life cycle.</summary>
public enum PayRunMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pay run metric as the pay run team keeps it.</summary>
public sealed class PayRunMetric(string code, string title, double threshold, string timezone, string channel, PayRunMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string Timezone { get; set; } = timezone;
    public string Channel { get; set; } = channel;
    public PayRunMetricStatus Status { get; set; } = status;

    /// <summary>Starts a pay run metric in status proposed.</summary>
    public static PayRunMetric NewPayRunMetric(string code, string title)
    {
        return new PayRunMetric(code, title, 0.0, "", "", PayRunMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunMetricStatus next)
    {
        switch (Status)
        {
            case PayRunMetricStatus.Proposed:
                switch (next)
                {
                    case PayRunMetricStatus.Rejected:
                    case PayRunMetricStatus.Withdrawn:
                        return true;
                }
                return false;
            case PayRunMetricStatus.Approved:
                switch (next)
                {
                    case PayRunMetricStatus.Withdrawn:
                        return true;
                }
                return false;
            case PayRunMetricStatus.Rejected:
                return next == PayRunMetricStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PayRunMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public PayRunMetric Copy()
    {
        return new PayRunMetric(Code, Title, Threshold, Timezone, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
