namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive payslip review is in its life cycle.</summary>
public enum AdaptivePayslipReviewStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One adaptive payslip review, with what the planners need to act on it.</summary>
public sealed class AdaptivePayslipReview(string code, string name, int priority, string timezone, double score, double maximum, string employeeId, int leaveDays, AdaptivePayslipReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public string Timezone { get; set; } = timezone;
    public double Score { get; set; } = score;
    public double Maximum { get; set; } = maximum;
    public string EmployeeId { get; set; } = employeeId;
    public int LeaveDays { get; set; } = leaveDays;
    public AdaptivePayslipReviewStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive payslip review in status requested.</summary>
    public static AdaptivePayslipReview NewAdaptivePayslipReview(string code, string name)
    {
        return new AdaptivePayslipReview(code, name, 0, "", 0.0, 0.0, "", 0, AdaptivePayslipReviewStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePayslipReviewStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePayslipReviewStatus next)
    {
        switch (Status)
        {
            case AdaptivePayslipReviewStatus.Requested:
                switch (next)
                {
                    case AdaptivePayslipReviewStatus.Revoked:
                    case AdaptivePayslipReviewStatus.Expired:
                        return true;
                }
                return false;
            case AdaptivePayslipReviewStatus.Granted:
                return next == AdaptivePayslipReviewStatus.Revoked;
            case AdaptivePayslipReviewStatus.Revoked:
                switch (next)
                {
                    case AdaptivePayslipReviewStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptivePayslipReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive payslip review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedValue()
    {
        return Math.Round(Maximum + (double)Priority, 2);
    }

    public AdaptivePayslipReview Copy()
    {
        return new AdaptivePayslipReview(
            Code,
            Name,
            Priority,
            Timezone,
            Score,
            Maximum,
            EmployeeId,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
