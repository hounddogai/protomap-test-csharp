namespace Acme.Workforce.Payrollops.Deduction.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction estimate is in its life cycle.</summary>
public enum DeductionEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A deduction estimate. Its code is unique within a tenant.</summary>
public sealed class DeductionEstimate(string code, string title, string badge, string timezone, int expiresAt, int attempts, string summary, string locale, string externalRef, double budget, int leaveDays, DeductionEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public string Timezone { get; set; } = timezone;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Attempts { get; set; } = attempts;
    public string Summary { get; set; } = summary;
    public string Locale { get; set; } = locale;
    public string ExternalRef { get; set; } = externalRef;
    public double Budget { get; set; } = budget;
    public int LeaveDays { get; set; } = leaveDays;
    public DeductionEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a deduction estimate in status planned.</summary>
    public static DeductionEstimate NewDeductionEstimate(string code, string title)
    {
        return new DeductionEstimate(code, title, "", "", 0, 0, "", "", "", 0.0, 0, DeductionEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionEstimateStatus next)
    {
        switch (Status)
        {
            case DeductionEstimateStatus.Planned:
                switch (next)
                {
                    case DeductionEstimateStatus.Scheduled:
                    case DeductionEstimateStatus.Completed:
                        return true;
                }
                return false;
            case DeductionEstimateStatus.Scheduled:
                switch (next)
                {
                    case DeductionEstimateStatus.Completed:
                    case DeductionEstimateStatus.Cancelled:
                        return true;
                }
                return false;
            case DeductionEstimateStatus.InProgress:
                return next == DeductionEstimateStatus.Completed || next == DeductionEstimateStatus.Cancelled;
            case DeductionEstimateStatus.Completed:
                switch (next)
                {
                    case DeductionEstimateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"deduction estimate"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)ExpiresAt * Budget, 2);
    }

    public DeductionEstimate Copy()
    {
        return new DeductionEstimate(
            Code,
            Title,
            Badge,
            Timezone,
            ExpiresAt,
            Attempts,
            Summary,
            Locale,
            ExternalRef,
            Budget,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
