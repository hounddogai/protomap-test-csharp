namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip estimate is in its life cycle.</summary>
public enum PayslipEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a payslip estimate and its current status.</summary>
public sealed class PayslipEstimate(string code, string name, int expiresAt, int durationMinutes, string timezone, string channel, bool active, string summary, PayslipEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Timezone { get; set; } = timezone;
    public string Channel { get; set; } = channel;
    public bool Active { get; set; } = active;
    public string Summary { get; set; } = summary;
    public PayslipEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a payslip estimate in status proposed.</summary>
    public static PayslipEstimate NewPayslipEstimate(string code, string name)
    {
        return new PayslipEstimate(code, name, 0, 0, "", "", false, "", PayslipEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipEstimateStatus next)
    {
        switch (Status)
        {
            case PayslipEstimateStatus.Proposed:
                return next == PayslipEstimateStatus.Rejected || next == PayslipEstimateStatus.Withdrawn;
            case PayslipEstimateStatus.Approved:
                return next == PayslipEstimateStatus.Rejected;
            case PayslipEstimateStatus.Rejected:
                return next == PayslipEstimateStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PayslipEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"payslip estimate"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)ExpiresAt + (double)DurationMinutes, 2);
    }

    public PayslipEstimate Copy()
    {
        return new PayslipEstimate(
            Code,
            Name,
            ExpiresAt,
            DurationMinutes,
            Timezone,
            Channel,
            Active,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
