namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan batch is in its life cycle.</summary>
public enum FloorPlanBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A floor plan batch as the floor plan team keeps it.</summary>
public sealed class FloorPlanBatch(string code, string name, double discount, string grade, string checksum, string phone, bool visible, double hours, FloorPlanBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public string Grade { get; set; } = grade;
    public string Checksum { get; set; } = checksum;
    public string Phone { get; set; } = phone;
    public bool Visible { get; set; } = visible;
    public double Hours { get; set; } = hours;
    public FloorPlanBatchStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan batch in status created.</summary>
    public static FloorPlanBatch NewFloorPlanBatch(string code, string name)
    {
        return new FloorPlanBatch(code, name, 0.0, "", "", "", false, 0.0, FloorPlanBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanBatchStatus next)
    {
        switch (Status)
        {
            case FloorPlanBatchStatus.Created:
                switch (next)
                {
                    case FloorPlanBatchStatus.Published:
                        return true;
                }
                return false;
            case FloorPlanBatchStatus.Validated:
                return next == FloorPlanBatchStatus.Published;
            case FloorPlanBatchStatus.Published:
                return next == FloorPlanBatchStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FloorPlanBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"floor plan batch"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Hours - Discount, 2);
    }

    public FloorPlanBatch Copy()
    {
        return new FloorPlanBatch(Code, Name, Discount, Grade, Checksum, Phone, Visible, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
