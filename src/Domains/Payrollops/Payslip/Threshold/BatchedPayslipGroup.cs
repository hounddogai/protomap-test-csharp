namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched payslip group is in its life cycle.</summary>
public enum BatchedPayslipGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A batched payslip group as the payslip team keeps it.</summary>
public sealed class BatchedPayslipGroup(string code, string name, string team, int durationMinutes, bool visible, int tenureMonths, string source, double ratio, int priority, double score, string contactEmail, BatchedPayslipGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Visible { get; set; } = visible;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Source { get; set; } = source;
    public double Ratio { get; set; } = ratio;
    public int Priority { get; set; } = priority;
    public double Score { get; set; } = score;
    public string ContactEmail { get; set; } = contactEmail;
    public BatchedPayslipGroupStatus Status { get; set; } = status;

    /// <summary>Starts a batched payslip group in status idle.</summary>
    public static BatchedPayslipGroup NewBatchedPayslipGroup(string code, string name)
    {
        return new BatchedPayslipGroup(
            code,
            name,
            "",
            0,
            false,
            0,
            "",
            0.0,
            0,
            0.0,
            "",
            BatchedPayslipGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPayslipGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPayslipGroupStatus next)
    {
        switch (Status)
        {
            case BatchedPayslipGroupStatus.Idle:
                switch (next)
                {
                    case BatchedPayslipGroupStatus.Warming:
                        return true;
                }
                return false;
            case BatchedPayslipGroupStatus.Warming:
                return next == BatchedPayslipGroupStatus.Draining;
            case BatchedPayslipGroupStatus.Ready:
                return next == BatchedPayslipGroupStatus.Draining || next == BatchedPayslipGroupStatus.Stopped;
            case BatchedPayslipGroupStatus.Draining:
                return next == BatchedPayslipGroupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BatchedPayslipGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched payslip group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Priority - (double)DurationMinutes, 2);
    }

    public BatchedPayslipGroup Copy()
    {
        return new BatchedPayslipGroup(
            Code,
            Name,
            Team,
            DurationMinutes,
            Visible,
            TenureMonths,
            Source,
            Ratio,
            Priority,
            Score,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
