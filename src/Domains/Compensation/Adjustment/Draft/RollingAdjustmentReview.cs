namespace Acme.Workforce.Compensation.Adjustment.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling adjustment review is in its life cycle.</summary>
public enum RollingAdjustmentReviewStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A rolling adjustment review as the adjustment team keeps it.</summary>
public sealed class RollingAdjustmentReview(string code, string label, string team, string externalRef, string managerId, string salaryBand, double rate, int leaveDays, string timezone, string jobCode, string channel, RollingAdjustmentReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public string ExternalRef { get; set; } = externalRef;
    public string ManagerId { get; set; } = managerId;
    public string SalaryBand { get; set; } = salaryBand;
    public double Rate { get; set; } = rate;
    public int LeaveDays { get; set; } = leaveDays;
    public string Timezone { get; set; } = timezone;
    public string JobCode { get; set; } = jobCode;
    public string Channel { get; set; } = channel;
    public RollingAdjustmentReviewStatus Status { get; set; } = status;

    /// <summary>Starts a rolling adjustment review in status new.</summary>
    public static RollingAdjustmentReview NewRollingAdjustmentReview(string code, string label)
    {
        return new RollingAdjustmentReview(
            code,
            label,
            "",
            "",
            "",
            "",
            0.0,
            0,
            "",
            "",
            "",
            RollingAdjustmentReviewStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAdjustmentReviewStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAdjustmentReviewStatus next)
    {
        switch (Status)
        {
            case RollingAdjustmentReviewStatus.New:
                return next == RollingAdjustmentReviewStatus.Queued || next == RollingAdjustmentReviewStatus.Done;
            case RollingAdjustmentReviewStatus.Queued:
                switch (next)
                {
                    case RollingAdjustmentReviewStatus.Done:
                    case RollingAdjustmentReviewStatus.Failed:
                        return true;
                }
                return false;
            case RollingAdjustmentReviewStatus.Running:
                return next == RollingAdjustmentReviewStatus.Failed;
            case RollingAdjustmentReviewStatus.Done:
                return next == RollingAdjustmentReviewStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RollingAdjustmentReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling adjustment review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling adjustment review"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)LeaveDays * Rate, 2);
    }

    public RollingAdjustmentReview Copy()
    {
        return new RollingAdjustmentReview(
            Code,
            Label,
            Team,
            ExternalRef,
            ManagerId,
            SalaryBand,
            Rate,
            LeaveDays,
            Timezone,
            JobCode,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
