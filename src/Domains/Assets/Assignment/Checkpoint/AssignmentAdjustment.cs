namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment adjustment is in its life cycle.</summary>
public enum AssignmentAdjustmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A assignment adjustment. Its code is unique within a tenant.</summary>
public sealed class AssignmentAdjustment(string code, string name, double amount, string title, int durationMinutes, int expiresAt, double margin, string salaryBand, double fte, double weight, AssignmentAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Margin { get; set; } = margin;
    public string SalaryBand { get; set; } = salaryBand;
    public double Fte { get; set; } = fte;
    public double Weight { get; set; } = weight;
    public AssignmentAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a assignment adjustment in status new.</summary>
    public static AssignmentAdjustment NewAssignmentAdjustment(string code, string name)
    {
        return new AssignmentAdjustment(
            code,
            name,
            0.0,
            "",
            0,
            0,
            0.0,
            "",
            0.0,
            0.0,
            AssignmentAdjustmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentAdjustmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentAdjustmentStatus next)
    {
        switch (Status)
        {
            case AssignmentAdjustmentStatus.New:
                switch (next)
                {
                    case AssignmentAdjustmentStatus.Queued:
                        return true;
                }
                return false;
            case AssignmentAdjustmentStatus.Queued:
                return next == AssignmentAdjustmentStatus.Running || next == AssignmentAdjustmentStatus.Done;
            case AssignmentAdjustmentStatus.Running:
                switch (next)
                {
                    case AssignmentAdjustmentStatus.Done:
                    case AssignmentAdjustmentStatus.Failed:
                        return true;
                }
                return false;
            case AssignmentAdjustmentStatus.Done:
                return next == AssignmentAdjustmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AssignmentAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"assignment adjustment"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)DurationMinutes - Margin, 2);
    }

    public AssignmentAdjustment Copy()
    {
        return new AssignmentAdjustment(
            Code,
            Name,
            Amount,
            Title,
            DurationMinutes,
            ExpiresAt,
            Margin,
            SalaryBand,
            Fte,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
