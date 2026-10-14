namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey metric is in its life cycle.</summary>
public enum PulseSurveyMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A pulse survey metric. Its code is unique within a tenant.</summary>
public sealed class PulseSurveyMetric(string code, string label, double margin, string department, double hours, int version, string costCenter, double discount, string jobCode, PulseSurveyMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public string Department { get; set; } = department;
    public double Hours { get; set; } = hours;
    public int Version { get; set; } = version;
    public string CostCenter { get; set; } = costCenter;
    public double Discount { get; set; } = discount;
    public string JobCode { get; set; } = jobCode;
    public PulseSurveyMetricStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey metric in status open.</summary>
    public static PulseSurveyMetric NewPulseSurveyMetric(string code, string label)
    {
        return new PulseSurveyMetric(code, label, 0.0, "", 0.0, 0, "", 0.0, "", PulseSurveyMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyMetricStatus next)
    {
        switch (Status)
        {
            case PulseSurveyMetricStatus.Open:
                switch (next)
                {
                    case PulseSurveyMetricStatus.Reopened:
                        return true;
                }
                return false;
            case PulseSurveyMetricStatus.InReview:
                switch (next)
                {
                    case PulseSurveyMetricStatus.Resolved:
                    case PulseSurveyMetricStatus.Archived:
                        return true;
                }
                return false;
            case PulseSurveyMetricStatus.Resolved:
                switch (next)
                {
                    case PulseSurveyMetricStatus.Reopened:
                    case PulseSurveyMetricStatus.Archived:
                        return true;
                }
                return false;
            case PulseSurveyMetricStatus.Reopened:
                switch (next)
                {
                    case PulseSurveyMetricStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(Margin - Discount, 2);
    }

    public PulseSurveyMetric Copy()
    {
        return new PulseSurveyMetric(
            Code,
            Label,
            Margin,
            Department,
            Hours,
            Version,
            CostCenter,
            Discount,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
