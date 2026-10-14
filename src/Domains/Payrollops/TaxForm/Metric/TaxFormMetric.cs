namespace Acme.Workforce.Payrollops.TaxForm.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form metric is in its life cycle.</summary>
public enum TaxFormMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a tax form metric and its current status.</summary>
public sealed class TaxFormMetric(string code, string label, int durationMinutes, string currency, string locale, TaxFormMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Currency { get; set; } = currency;
    public string Locale { get; set; } = locale;
    public TaxFormMetricStatus Status { get; set; } = status;

    /// <summary>Starts a tax form metric in status idle.</summary>
    public static TaxFormMetric NewTaxFormMetric(string code, string label)
    {
        return new TaxFormMetric(code, label, 0, "", "", TaxFormMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormMetricStatus next)
    {
        switch (Status)
        {
            case TaxFormMetricStatus.Idle:
                switch (next)
                {
                    case TaxFormMetricStatus.Stopped:
                        return true;
                }
                return false;
            case TaxFormMetricStatus.Warming:
                return next == TaxFormMetricStatus.Draining;
            case TaxFormMetricStatus.Ready:
                return next == TaxFormMetricStatus.Draining;
            case TaxFormMetricStatus.Draining:
                return next == TaxFormMetricStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TaxFormMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"tax form metric"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public TaxFormMetric Copy()
    {
        return new TaxFormMetric(Code, Label, DurationMinutes, Currency, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
