namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback tax form metric is in its life cycle.</summary>
public enum FallbackTaxFormMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One fallback tax form metric, with what the planners need to act on it.</summary>
public sealed class FallbackTaxFormMetric(string code, string label, string notes, string locale, string timezone, double cost, FallbackTaxFormMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string Locale { get; set; } = locale;
    public string Timezone { get; set; } = timezone;
    public double Cost { get; set; } = cost;
    public FallbackTaxFormMetricStatus Status { get; set; } = status;

    /// <summary>Starts a fallback tax form metric in status requested.</summary>
    public static FallbackTaxFormMetric NewFallbackTaxFormMetric(string code, string label)
    {
        return new FallbackTaxFormMetric(code, label, "", "", "", 0.0, FallbackTaxFormMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackTaxFormMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackTaxFormMetricStatus next)
    {
        switch (Status)
        {
            case FallbackTaxFormMetricStatus.Requested:
                switch (next)
                {
                    case FallbackTaxFormMetricStatus.Granted:
                    case FallbackTaxFormMetricStatus.Expired:
                        return true;
                }
                return false;
            case FallbackTaxFormMetricStatus.Granted:
                switch (next)
                {
                    case FallbackTaxFormMetricStatus.Revoked:
                        return true;
                }
                return false;
            case FallbackTaxFormMetricStatus.Revoked:
                return next == FallbackTaxFormMetricStatus.Expired;
        }
        return false;
    }

    public void MoveTo(FallbackTaxFormMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback tax form metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public FallbackTaxFormMetric Copy()
    {
        return new FallbackTaxFormMetric(Code, Label, Notes, Locale, Timezone, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
