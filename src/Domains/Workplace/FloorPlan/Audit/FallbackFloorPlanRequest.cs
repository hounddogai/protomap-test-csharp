namespace Acme.Workforce.Workplace.FloorPlan.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback floor plan request is in its life cycle.</summary>
public enum FallbackFloorPlanRequestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A fallback floor plan request as the floor plan team keeps it.</summary>
public sealed class FallbackFloorPlanRequest(string code, string label, int leaveDays, double weight, string title, FallbackFloorPlanRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public double Weight { get; set; } = weight;
    public string Title { get; set; } = title;
    public FallbackFloorPlanRequestStatus Status { get; set; } = status;

    /// <summary>Starts a fallback floor plan request in status open.</summary>
    public static FallbackFloorPlanRequest NewFallbackFloorPlanRequest(string code, string label)
    {
        return new FallbackFloorPlanRequest(code, label, 0, 0.0, "", FallbackFloorPlanRequestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackFloorPlanRequestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackFloorPlanRequestStatus next)
    {
        switch (Status)
        {
            case FallbackFloorPlanRequestStatus.Open:
                switch (next)
                {
                    case FallbackFloorPlanRequestStatus.InReview:
                        return true;
                }
                return false;
            case FallbackFloorPlanRequestStatus.InReview:
                return next == FallbackFloorPlanRequestStatus.Reopened || next == FallbackFloorPlanRequestStatus.Archived;
            case FallbackFloorPlanRequestStatus.Resolved:
                return next == FallbackFloorPlanRequestStatus.Archived;
            case FallbackFloorPlanRequestStatus.Reopened:
                return next == FallbackFloorPlanRequestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FallbackFloorPlanRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback floor plan request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback floor plan request"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)LeaveDays - Weight, 2);
    }

    public FallbackFloorPlanRequest Copy()
    {
        return new FallbackFloorPlanRequest(Code, Label, LeaveDays, Weight, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
