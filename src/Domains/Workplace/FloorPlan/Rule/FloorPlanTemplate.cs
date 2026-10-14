namespace Acme.Workforce.Workplace.FloorPlan.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan template is in its life cycle.</summary>
public enum FloorPlanTemplateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a floor plan template and its current status.</summary>
public sealed class FloorPlanTemplate(string code, string label, int retries, string grade, bool pinned, string costCenter, double score, int endsAt, int capacity, string notes, FloorPlanTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public string Grade { get; set; } = grade;
    public bool Pinned { get; set; } = pinned;
    public string CostCenter { get; set; } = costCenter;
    public double Score { get; set; } = score;
    public int EndsAt { get; set; } = endsAt;
    public int Capacity { get; set; } = capacity;
    public string Notes { get; set; } = notes;
    public FloorPlanTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan template in status requested.</summary>
    public static FloorPlanTemplate NewFloorPlanTemplate(string code, string label)
    {
        return new FloorPlanTemplate(
            code,
            label,
            0,
            "",
            false,
            "",
            0.0,
            0,
            0,
            "",
            FloorPlanTemplateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanTemplateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanTemplateStatus next)
    {
        switch (Status)
        {
            case FloorPlanTemplateStatus.Requested:
                switch (next)
                {
                    case FloorPlanTemplateStatus.Revoked:
                    case FloorPlanTemplateStatus.Expired:
                        return true;
                }
                return false;
            case FloorPlanTemplateStatus.Granted:
                return next == FloorPlanTemplateStatus.Expired;
            case FloorPlanTemplateStatus.Revoked:
                return next == FloorPlanTemplateStatus.Expired;
        }
        return false;
    }

    public void MoveTo(FloorPlanTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Capacity - Score, 2);
    }

    public FloorPlanTemplate Copy()
    {
        return new FloorPlanTemplate(
            Code,
            Label,
            Retries,
            Grade,
            Pinned,
            CostCenter,
            Score,
            EndsAt,
            Capacity,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
