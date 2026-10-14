namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default reorg plan is in its life cycle.</summary>
public enum DefaultReorgPlanStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One default reorg plan, with what the planners need to act on it.</summary>
public sealed class DefaultReorgPlan(string code, string title, string ownerId, int sequence, int limit, int startsAt, double amount, string managerId, bool archived, double cost, DefaultReorgPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public int Sequence { get; set; } = sequence;
    public int Limit { get; set; } = limit;
    public int StartsAt { get; set; } = startsAt;
    public double Amount { get; set; } = amount;
    public string ManagerId { get; set; } = managerId;
    public bool Archived { get; set; } = archived;
    public double Cost { get; set; } = cost;
    public DefaultReorgPlanStatus Status { get; set; } = status;

    /// <summary>Starts a default reorg plan in status proposed.</summary>
    public static DefaultReorgPlan NewDefaultReorgPlan(string code, string title)
    {
        return new DefaultReorgPlan(code, title, "", 0, 0, 0, 0.0, "", false, 0.0, DefaultReorgPlanStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultReorgPlanStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultReorgPlanStatus next)
    {
        switch (Status)
        {
            case DefaultReorgPlanStatus.Proposed:
                switch (next)
                {
                    case DefaultReorgPlanStatus.Approved:
                        return true;
                }
                return false;
            case DefaultReorgPlanStatus.Approved:
                switch (next)
                {
                    case DefaultReorgPlanStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultReorgPlanStatus.Rejected:
                return next == DefaultReorgPlanStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DefaultReorgPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default reorg plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetValue()
    {
        return Math.Round((double)Sequence - Cost, 2);
    }

    public DefaultReorgPlan Copy()
    {
        return new DefaultReorgPlan(
            Code,
            Title,
            OwnerId,
            Sequence,
            Limit,
            StartsAt,
            Amount,
            ManagerId,
            Archived,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
