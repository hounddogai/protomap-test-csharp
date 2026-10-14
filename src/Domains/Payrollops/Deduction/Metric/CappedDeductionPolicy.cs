namespace Acme.Workforce.Payrollops.Deduction.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped deduction policy is in its life cycle.</summary>
public enum CappedDeductionPolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One capped deduction policy, with what the planners need to act on it.</summary>
public sealed class CappedDeductionPolicy(string code, string name, string phone, int windowDays, string description, int version, string category, string currency, double score, string employeeId, CappedDeductionPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public int WindowDays { get; set; } = windowDays;
    public string Description { get; set; } = description;
    public int Version { get; set; } = version;
    public string Category { get; set; } = category;
    public string Currency { get; set; } = currency;
    public double Score { get; set; } = score;
    public string EmployeeId { get; set; } = employeeId;
    public CappedDeductionPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a capped deduction policy in status planned.</summary>
    public static CappedDeductionPolicy NewCappedDeductionPolicy(string code, string name)
    {
        return new CappedDeductionPolicy(
            code,
            name,
            "",
            0,
            "",
            0,
            "",
            "",
            0.0,
            "",
            CappedDeductionPolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedDeductionPolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedDeductionPolicyStatus next)
    {
        switch (Status)
        {
            case CappedDeductionPolicyStatus.Planned:
                switch (next)
                {
                    case CappedDeductionPolicyStatus.Completed:
                        return true;
                }
                return false;
            case CappedDeductionPolicyStatus.Scheduled:
                return next == CappedDeductionPolicyStatus.Completed || next == CappedDeductionPolicyStatus.Cancelled;
            case CappedDeductionPolicyStatus.InProgress:
                return next == CappedDeductionPolicyStatus.Completed || next == CappedDeductionPolicyStatus.Cancelled;
            case CappedDeductionPolicyStatus.Completed:
                return next == CappedDeductionPolicyStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CappedDeductionPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped deduction policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"capped deduction policy"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Version + Score, 2);
    }

    public CappedDeductionPolicy Copy()
    {
        return new CappedDeductionPolicy(
            Code,
            Name,
            Phone,
            WindowDays,
            Description,
            Version,
            Category,
            Currency,
            Score,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
