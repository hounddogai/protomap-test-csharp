namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual adjustment rule is in its life cycle.</summary>
public enum ManualAdjustmentRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A manual adjustment rule as the adjustment team keeps it.</summary>
public sealed class ManualAdjustmentRule(string code, string title, string department, double floorValue, string employeeId, string salaryBand, double ceiling, int capacity, ManualAdjustmentRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public double FloorValue { get; set; } = floorValue;
    public string EmployeeId { get; set; } = employeeId;
    public string SalaryBand { get; set; } = salaryBand;
    public double Ceiling { get; set; } = ceiling;
    public int Capacity { get; set; } = capacity;
    public ManualAdjustmentRuleStatus Status { get; set; } = status;

    /// <summary>Starts a manual adjustment rule in status idle.</summary>
    public static ManualAdjustmentRule NewManualAdjustmentRule(string code, string title)
    {
        return new ManualAdjustmentRule(code, title, "", 0.0, "", "", 0.0, 0, ManualAdjustmentRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAdjustmentRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAdjustmentRuleStatus next)
    {
        switch (Status)
        {
            case ManualAdjustmentRuleStatus.Idle:
                switch (next)
                {
                    case ManualAdjustmentRuleStatus.Ready:
                    case ManualAdjustmentRuleStatus.Draining:
                        return true;
                }
                return false;
            case ManualAdjustmentRuleStatus.Warming:
                switch (next)
                {
                    case ManualAdjustmentRuleStatus.Ready:
                    case ManualAdjustmentRuleStatus.Draining:
                        return true;
                }
                return false;
            case ManualAdjustmentRuleStatus.Ready:
                switch (next)
                {
                    case ManualAdjustmentRuleStatus.Stopped:
                        return true;
                }
                return false;
            case ManualAdjustmentRuleStatus.Draining:
                switch (next)
                {
                    case ManualAdjustmentRuleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualAdjustmentRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual adjustment rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Capacity - FloorValue, 2);
    }

    public ManualAdjustmentRule Copy()
    {
        return new ManualAdjustmentRule(
            Code,
            Title,
            Department,
            FloorValue,
            EmployeeId,
            SalaryBand,
            Ceiling,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
