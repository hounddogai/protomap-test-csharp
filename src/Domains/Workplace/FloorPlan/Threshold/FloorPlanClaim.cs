namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan claim is in its life cycle.</summary>
public enum FloorPlanClaimStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One floor plan claim, with what the planners need to act on it.</summary>
public sealed class FloorPlanClaim(string code, string label, double score, string name, string notes, bool pinned, string skill, string title, double cost, string jobCode, FloorPlanClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public bool Pinned { get; set; } = pinned;
    public string Skill { get; set; } = skill;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public string JobCode { get; set; } = jobCode;
    public FloorPlanClaimStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan claim in status planned.</summary>
    public static FloorPlanClaim NewFloorPlanClaim(string code, string label)
    {
        return new FloorPlanClaim(code, label, 0.0, "", "", false, "", "", 0.0, "", FloorPlanClaimStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanClaimStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanClaimStatus next)
    {
        switch (Status)
        {
            case FloorPlanClaimStatus.Planned:
                return next == FloorPlanClaimStatus.Scheduled || next == FloorPlanClaimStatus.Completed;
            case FloorPlanClaimStatus.Scheduled:
                switch (next)
                {
                    case FloorPlanClaimStatus.InProgress:
                    case FloorPlanClaimStatus.Completed:
                        return true;
                }
                return false;
            case FloorPlanClaimStatus.InProgress:
                switch (next)
                {
                    case FloorPlanClaimStatus.Completed:
                    case FloorPlanClaimStatus.Cancelled:
                        return true;
                }
                return false;
            case FloorPlanClaimStatus.Completed:
                switch (next)
                {
                    case FloorPlanClaimStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Cost - Score, 2);
    }

    public FloorPlanClaim Copy()
    {
        return new FloorPlanClaim(Code, Label, Score, Name, Notes, Pinned, Skill, Title, Cost, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
