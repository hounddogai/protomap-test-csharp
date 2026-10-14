namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan adjustment is in its life cycle.</summary>
public enum FloorPlanAdjustmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A floor plan adjustment. Its code is unique within a tenant.</summary>
public sealed class FloorPlanAdjustment(string code, string label, int retries, int batchSize, string phone, string grade, bool pinned, FloorPlanAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public int BatchSize { get; set; } = batchSize;
    public string Phone { get; set; } = phone;
    public string Grade { get; set; } = grade;
    public bool Pinned { get; set; } = pinned;
    public FloorPlanAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan adjustment in status draft.</summary>
    public static FloorPlanAdjustment NewFloorPlanAdjustment(string code, string label)
    {
        return new FloorPlanAdjustment(code, label, 0, 0, "", "", false, FloorPlanAdjustmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanAdjustmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanAdjustmentStatus next)
    {
        switch (Status)
        {
            case FloorPlanAdjustmentStatus.Draft:
                switch (next)
                {
                    case FloorPlanAdjustmentStatus.Pending:
                    case FloorPlanAdjustmentStatus.Closed:
                        return true;
                }
                return false;
            case FloorPlanAdjustmentStatus.Pending:
                return next == FloorPlanAdjustmentStatus.Suspended || next == FloorPlanAdjustmentStatus.Closed;
            case FloorPlanAdjustmentStatus.Active:
                switch (next)
                {
                    case FloorPlanAdjustmentStatus.Suspended:
                    case FloorPlanAdjustmentStatus.Closed:
                        return true;
                }
                return false;
            case FloorPlanAdjustmentStatus.Suspended:
                switch (next)
                {
                    case FloorPlanAdjustmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Retries + (double)BatchSize, 2);
    }

    public FloorPlanAdjustment Copy()
    {
        return new FloorPlanAdjustment(Code, Label, Retries, BatchSize, Phone, Grade, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
