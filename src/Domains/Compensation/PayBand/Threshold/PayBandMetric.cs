namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band metric is in its life cycle.</summary>
public enum PayBandMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a pay band metric and its current status.</summary>
public sealed class PayBandMetric(string code, string title, double ceiling, double amount, double ratio, string externalRef, string segment, double margin, Dictionary<string, string> attributes, string badge, string phone, PayBandMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public double Amount { get; set; } = amount;
    public double Ratio { get; set; } = ratio;
    public string ExternalRef { get; set; } = externalRef;
    public string Segment { get; set; } = segment;
    public double Margin { get; set; } = margin;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Badge { get; set; } = badge;
    public string Phone { get; set; } = phone;
    public PayBandMetricStatus Status { get; set; } = status;

    /// <summary>Starts a pay band metric in status planned.</summary>
    public static PayBandMetric NewPayBandMetric(string code, string title)
    {
        return new PayBandMetric(
            code,
            title,
            0.0,
            0.0,
            0.0,
            "",
            "",
            0.0,
            new Dictionary<string, string>(),
            "",
            "",
            PayBandMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandMetricStatus next)
    {
        switch (Status)
        {
            case PayBandMetricStatus.Planned:
                switch (next)
                {
                    case PayBandMetricStatus.Cancelled:
                        return true;
                }
                return false;
            case PayBandMetricStatus.Scheduled:
                switch (next)
                {
                    case PayBandMetricStatus.Completed:
                        return true;
                }
                return false;
            case PayBandMetricStatus.InProgress:
                return next == PayBandMetricStatus.Completed || next == PayBandMetricStatus.Cancelled;
            case PayBandMetricStatus.Completed:
                switch (next)
                {
                    case PayBandMetricStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedVolume()
    {
        return Math.Round(Amount - Ratio, 2);
    }

    public PayBandMetric Copy()
    {
        return new PayBandMetric(
            Code,
            Title,
            Ceiling,
            Amount,
            Ratio,
            ExternalRef,
            Segment,
            Margin,
            Attributes,
            Badge,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
