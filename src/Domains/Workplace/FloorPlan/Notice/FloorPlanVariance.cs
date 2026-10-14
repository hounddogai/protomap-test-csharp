namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan variance is in its life cycle.</summary>
public enum FloorPlanVarianceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a floor plan variance and its current status.</summary>
public sealed class FloorPlanVariance(string code, string label, int sequence, int endsAt, int priority, string phone, bool pinned, string badge, double ratio, FloorPlanVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public int EndsAt { get; set; } = endsAt;
    public int Priority { get; set; } = priority;
    public string Phone { get; set; } = phone;
    public bool Pinned { get; set; } = pinned;
    public string Badge { get; set; } = badge;
    public double Ratio { get; set; } = ratio;
    public FloorPlanVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan variance in status open.</summary>
    public static FloorPlanVariance NewFloorPlanVariance(string code, string label)
    {
        return new FloorPlanVariance(code, label, 0, 0, 0, "", false, "", 0.0, FloorPlanVarianceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanVarianceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanVarianceStatus next)
    {
        switch (Status)
        {
            case FloorPlanVarianceStatus.Open:
                switch (next)
                {
                    case FloorPlanVarianceStatus.InReview:
                    case FloorPlanVarianceStatus.Archived:
                        return true;
                }
                return false;
            case FloorPlanVarianceStatus.InReview:
                return next == FloorPlanVarianceStatus.Reopened || next == FloorPlanVarianceStatus.Archived;
            case FloorPlanVarianceStatus.Resolved:
                switch (next)
                {
                    case FloorPlanVarianceStatus.Reopened:
                    case FloorPlanVarianceStatus.Archived:
                        return true;
                }
                return false;
            case FloorPlanVarianceStatus.Reopened:
                switch (next)
                {
                    case FloorPlanVarianceStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"floor plan variance"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Priority - Ratio, 2);
    }

    public FloorPlanVariance Copy()
    {
        return new FloorPlanVariance(Code, Label, Sequence, EndsAt, Priority, Phone, Pinned, Badge, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
