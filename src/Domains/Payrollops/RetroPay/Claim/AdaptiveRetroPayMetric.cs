namespace Acme.Workforce.Payrollops.RetroPay.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive retro pay metric is in its life cycle.</summary>
public enum AdaptiveRetroPayMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One adaptive retro pay metric, with what the planners need to act on it.</summary>
public sealed class AdaptiveRetroPayMetric(string code, string title, double budget, int leaveDays, double amount, double hours, AdaptiveRetroPayMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public int LeaveDays { get; set; } = leaveDays;
    public double Amount { get; set; } = amount;
    public double Hours { get; set; } = hours;
    public AdaptiveRetroPayMetricStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive retro pay metric in status created.</summary>
    public static AdaptiveRetroPayMetric NewAdaptiveRetroPayMetric(string code, string title)
    {
        return new AdaptiveRetroPayMetric(code, title, 0.0, 0, 0.0, 0.0, AdaptiveRetroPayMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveRetroPayMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveRetroPayMetricStatus next)
    {
        switch (Status)
        {
            case AdaptiveRetroPayMetricStatus.Created:
                switch (next)
                {
                    case AdaptiveRetroPayMetricStatus.Validated:
                    case AdaptiveRetroPayMetricStatus.Published:
                        return true;
                }
                return false;
            case AdaptiveRetroPayMetricStatus.Validated:
                return next == AdaptiveRetroPayMetricStatus.Published;
            case AdaptiveRetroPayMetricStatus.Published:
                switch (next)
                {
                    case AdaptiveRetroPayMetricStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveRetroPayMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive retro pay metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"adaptive retro pay metric"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Budget - Hours, 2);
    }

    public AdaptiveRetroPayMetric Copy()
    {
        return new AdaptiveRetroPayMetric(Code, Title, Budget, LeaveDays, Amount, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
