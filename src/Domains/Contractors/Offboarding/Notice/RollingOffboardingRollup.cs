namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling offboarding rollup is in its life cycle.</summary>
public enum RollingOffboardingRollupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One rolling offboarding rollup, with what the planners need to act on it.</summary>
public sealed class RollingOffboardingRollup(string code, string label, double threshold, string costCenter, int tenureMonths, double score, RollingOffboardingRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public string CostCenter { get; set; } = costCenter;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Score { get; set; } = score;
    public RollingOffboardingRollupStatus Status { get; set; } = status;

    /// <summary>Starts a rolling offboarding rollup in status draft.</summary>
    public static RollingOffboardingRollup NewRollingOffboardingRollup(string code, string label)
    {
        return new RollingOffboardingRollup(code, label, 0.0, "", 0, 0.0, RollingOffboardingRollupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingOffboardingRollupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingOffboardingRollupStatus next)
    {
        switch (Status)
        {
            case RollingOffboardingRollupStatus.Draft:
                return next == RollingOffboardingRollupStatus.Pending || next == RollingOffboardingRollupStatus.Closed;
            case RollingOffboardingRollupStatus.Pending:
                return next == RollingOffboardingRollupStatus.Closed;
            case RollingOffboardingRollupStatus.Active:
                switch (next)
                {
                    case RollingOffboardingRollupStatus.Suspended:
                    case RollingOffboardingRollupStatus.Closed:
                        return true;
                }
                return false;
            case RollingOffboardingRollupStatus.Suspended:
                return next == RollingOffboardingRollupStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RollingOffboardingRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling offboarding rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling offboarding rollup"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Score - (double)TenureMonths, 2);
    }

    public RollingOffboardingRollup Copy()
    {
        return new RollingOffboardingRollup(Code, Label, Threshold, CostCenter, TenureMonths, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
