namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition budget is in its life cycle.</summary>
public enum RecognitionBudgetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A recognition budget as the recognition team keeps it.</summary>
public sealed class RecognitionBudget(string code, string label, int version, int capacity, string jobCode, double threshold, string costCenter, string description, string employeeId, string unit, double floorValue, RecognitionBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public int Capacity { get; set; } = capacity;
    public string JobCode { get; set; } = jobCode;
    public double Threshold { get; set; } = threshold;
    public string CostCenter { get; set; } = costCenter;
    public string Description { get; set; } = description;
    public string EmployeeId { get; set; } = employeeId;
    public string Unit { get; set; } = unit;
    public double FloorValue { get; set; } = floorValue;
    public RecognitionBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a recognition budget in status proposed.</summary>
    public static RecognitionBudget NewRecognitionBudget(string code, string label)
    {
        return new RecognitionBudget(
            code,
            label,
            0,
            0,
            "",
            0.0,
            "",
            "",
            "",
            "",
            0.0,
            RecognitionBudgetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionBudgetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionBudgetStatus next)
    {
        switch (Status)
        {
            case RecognitionBudgetStatus.Proposed:
                switch (next)
                {
                    case RecognitionBudgetStatus.Rejected:
                        return true;
                }
                return false;
            case RecognitionBudgetStatus.Approved:
                switch (next)
                {
                    case RecognitionBudgetStatus.Rejected:
                    case RecognitionBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
            case RecognitionBudgetStatus.Rejected:
                switch (next)
                {
                    case RecognitionBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Threshold + (double)Version, 2);
    }

    public RecognitionBudget Copy()
    {
        return new RecognitionBudget(
            Code,
            Label,
            Version,
            Capacity,
            JobCode,
            Threshold,
            CostCenter,
            Description,
            EmployeeId,
            Unit,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
