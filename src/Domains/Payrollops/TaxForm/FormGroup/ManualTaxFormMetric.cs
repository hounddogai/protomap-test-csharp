namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual tax form metric is in its life cycle.</summary>
public enum ManualTaxFormMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a manual tax form metric and its current status.</summary>
public sealed class ManualTaxFormMetric(string code, string title, int expiresAt, string category, bool active, double rating, string channel, string jobCode, ManualTaxFormMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Category { get; set; } = category;
    public bool Active { get; set; } = active;
    public double Rating { get; set; } = rating;
    public string Channel { get; set; } = channel;
    public string JobCode { get; set; } = jobCode;
    public ManualTaxFormMetricStatus Status { get; set; } = status;

    /// <summary>Starts a manual tax form metric in status open.</summary>
    public static ManualTaxFormMetric NewManualTaxFormMetric(string code, string title)
    {
        return new ManualTaxFormMetric(code, title, 0, "", false, 0.0, "", "", ManualTaxFormMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualTaxFormMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualTaxFormMetricStatus next)
    {
        switch (Status)
        {
            case ManualTaxFormMetricStatus.Open:
                switch (next)
                {
                    case ManualTaxFormMetricStatus.Archived:
                        return true;
                }
                return false;
            case ManualTaxFormMetricStatus.InReview:
                switch (next)
                {
                    case ManualTaxFormMetricStatus.Resolved:
                    case ManualTaxFormMetricStatus.Reopened:
                        return true;
                }
                return false;
            case ManualTaxFormMetricStatus.Resolved:
                return next == ManualTaxFormMetricStatus.Archived;
            case ManualTaxFormMetricStatus.Reopened:
                return next == ManualTaxFormMetricStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ManualTaxFormMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual tax form metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"manual tax form metric"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)ExpiresAt * Rating, 2);
    }

    public ManualTaxFormMetric Copy()
    {
        return new ManualTaxFormMetric(Code, Title, ExpiresAt, Category, Active, Rating, Channel, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
