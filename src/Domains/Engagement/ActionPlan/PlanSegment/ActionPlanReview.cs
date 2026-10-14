namespace Acme.Workforce.Engagement.ActionPlan.PlanSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan review is in its life cycle.</summary>
public enum ActionPlanReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One action plan review, with what the planners need to act on it.</summary>
public sealed class ActionPlanReview(string code, string label, double weight, double maximum, string category, string reviewer, double ceiling, ActionPlanReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public double Maximum { get; set; } = maximum;
    public string Category { get; set; } = category;
    public string Reviewer { get; set; } = reviewer;
    public double Ceiling { get; set; } = ceiling;
    public ActionPlanReviewStatus Status { get; set; } = status;

    /// <summary>Starts a action plan review in status planned.</summary>
    public static ActionPlanReview NewActionPlanReview(string code, string label)
    {
        return new ActionPlanReview(code, label, 0.0, 0.0, "", "", 0.0, ActionPlanReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanReviewStatus next)
    {
        switch (Status)
        {
            case ActionPlanReviewStatus.Planned:
                switch (next)
                {
                    case ActionPlanReviewStatus.InProgress:
                        return true;
                }
                return false;
            case ActionPlanReviewStatus.Scheduled:
                switch (next)
                {
                    case ActionPlanReviewStatus.InProgress:
                    case ActionPlanReviewStatus.Cancelled:
                        return true;
                }
                return false;
            case ActionPlanReviewStatus.InProgress:
                return next == ActionPlanReviewStatus.Completed || next == ActionPlanReviewStatus.Cancelled;
            case ActionPlanReviewStatus.Completed:
                switch (next)
                {
                    case ActionPlanReviewStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Ceiling * Maximum, 2);
    }

    public ActionPlanReview Copy()
    {
        return new ActionPlanReview(Code, Label, Weight, Maximum, Category, Reviewer, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
