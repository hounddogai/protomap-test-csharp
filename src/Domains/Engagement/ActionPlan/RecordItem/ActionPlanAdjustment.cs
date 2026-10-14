namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan adjustment is in its life cycle.</summary>
public enum ActionPlanAdjustmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A action plan adjustment as the action plan team keeps it.</summary>
public sealed class ActionPlanAdjustment(string code, string title, int graceDays, double ratio, string country, double discount, string timezone, string ownerId, int endsAt, string reviewer, string managerId, ActionPlanAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public double Ratio { get; set; } = ratio;
    public string Country { get; set; } = country;
    public double Discount { get; set; } = discount;
    public string Timezone { get; set; } = timezone;
    public string OwnerId { get; set; } = ownerId;
    public int EndsAt { get; set; } = endsAt;
    public string Reviewer { get; set; } = reviewer;
    public string ManagerId { get; set; } = managerId;
    public ActionPlanAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a action plan adjustment in status proposed.</summary>
    public static ActionPlanAdjustment NewActionPlanAdjustment(string code, string title)
    {
        return new ActionPlanAdjustment(
            code,
            title,
            0,
            0.0,
            "",
            0.0,
            "",
            "",
            0,
            "",
            "",
            ActionPlanAdjustmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanAdjustmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanAdjustmentStatus next)
    {
        switch (Status)
        {
            case ActionPlanAdjustmentStatus.Proposed:
                return next == ActionPlanAdjustmentStatus.Approved || next == ActionPlanAdjustmentStatus.Withdrawn;
            case ActionPlanAdjustmentStatus.Approved:
                return next == ActionPlanAdjustmentStatus.Rejected;
            case ActionPlanAdjustmentStatus.Rejected:
                return next == ActionPlanAdjustmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ActionPlanAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"action plan adjustment"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Ratio * (double)EndsAt, 2);
    }

    public ActionPlanAdjustment Copy()
    {
        return new ActionPlanAdjustment(
            Code,
            Title,
            GraceDays,
            Ratio,
            Country,
            Discount,
            Timezone,
            OwnerId,
            EndsAt,
            Reviewer,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
