namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback recognition plan is in its life cycle.</summary>
public enum FallbackRecognitionPlanStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One fallback recognition plan, with what the planners need to act on it.</summary>
public sealed class FallbackRecognitionPlan(string code, string label, string segment, double discount, string ownerId, FallbackRecognitionPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public double Discount { get; set; } = discount;
    public string OwnerId { get; set; } = ownerId;
    public FallbackRecognitionPlanStatus Status { get; set; } = status;

    /// <summary>Starts a fallback recognition plan in status planned.</summary>
    public static FallbackRecognitionPlan NewFallbackRecognitionPlan(string code, string label)
    {
        return new FallbackRecognitionPlan(code, label, "", 0.0, "", FallbackRecognitionPlanStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackRecognitionPlanStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackRecognitionPlanStatus next)
    {
        switch (Status)
        {
            case FallbackRecognitionPlanStatus.Planned:
                switch (next)
                {
                    case FallbackRecognitionPlanStatus.Completed:
                        return true;
                }
                return false;
            case FallbackRecognitionPlanStatus.Scheduled:
                return next == FallbackRecognitionPlanStatus.InProgress;
            case FallbackRecognitionPlanStatus.InProgress:
                switch (next)
                {
                    case FallbackRecognitionPlanStatus.Completed:
                    case FallbackRecognitionPlanStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackRecognitionPlanStatus.Completed:
                switch (next)
                {
                    case FallbackRecognitionPlanStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackRecognitionPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback recognition plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback recognition plan"} {Code}";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public FallbackRecognitionPlan Copy()
    {
        return new FallbackRecognitionPlan(Code, Label, Segment, Discount, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
