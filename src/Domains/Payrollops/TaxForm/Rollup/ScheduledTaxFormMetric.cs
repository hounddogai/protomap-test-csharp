namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled tax form metric is in its life cycle.</summary>
public enum ScheduledTaxFormMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One scheduled tax form metric, with what the planners need to act on it.</summary>
public sealed class ScheduledTaxFormMetric(string code, string name, string source, string notes, string contactEmail, double amount, ScheduledTaxFormMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public string Notes { get; set; } = notes;
    public string ContactEmail { get; set; } = contactEmail;
    public double Amount { get; set; } = amount;
    public ScheduledTaxFormMetricStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled tax form metric in status planned.</summary>
    public static ScheduledTaxFormMetric NewScheduledTaxFormMetric(string code, string name)
    {
        return new ScheduledTaxFormMetric(code, name, "", "", "", 0.0, ScheduledTaxFormMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledTaxFormMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledTaxFormMetricStatus next)
    {
        switch (Status)
        {
            case ScheduledTaxFormMetricStatus.Planned:
                switch (next)
                {
                    case ScheduledTaxFormMetricStatus.InProgress:
                        return true;
                }
                return false;
            case ScheduledTaxFormMetricStatus.Scheduled:
                return next == ScheduledTaxFormMetricStatus.Cancelled;
            case ScheduledTaxFormMetricStatus.InProgress:
                return next == ScheduledTaxFormMetricStatus.Completed || next == ScheduledTaxFormMetricStatus.Cancelled;
            case ScheduledTaxFormMetricStatus.Completed:
                return next == ScheduledTaxFormMetricStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledTaxFormMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled tax form metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public ScheduledTaxFormMetric Copy()
    {
        return new ScheduledTaxFormMetric(Code, Name, Source, Notes, ContactEmail, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
