namespace Acme.Workforce.Compensation.BonusPool.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool plan is in its life cycle.</summary>
public enum BonusPoolPlanStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a bonus pool plan and its current status.</summary>
public sealed class BonusPoolPlan(string code, string name, bool active, double rating, string segment, double ratio, double discount, int durationMinutes, BonusPoolPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public double Rating { get; set; } = rating;
    public string Segment { get; set; } = segment;
    public double Ratio { get; set; } = ratio;
    public double Discount { get; set; } = discount;
    public int DurationMinutes { get; set; } = durationMinutes;
    public BonusPoolPlanStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool plan in status open.</summary>
    public static BonusPoolPlan NewBonusPoolPlan(string code, string name)
    {
        return new BonusPoolPlan(code, name, false, 0.0, "", 0.0, 0.0, 0, BonusPoolPlanStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolPlanStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolPlanStatus next)
    {
        switch (Status)
        {
            case BonusPoolPlanStatus.Open:
                switch (next)
                {
                    case BonusPoolPlanStatus.Resolved:
                        return true;
                }
                return false;
            case BonusPoolPlanStatus.InReview:
                switch (next)
                {
                    case BonusPoolPlanStatus.Reopened:
                        return true;
                }
                return false;
            case BonusPoolPlanStatus.Resolved:
                switch (next)
                {
                    case BonusPoolPlanStatus.Archived:
                        return true;
                }
                return false;
            case BonusPoolPlanStatus.Reopened:
                return next == BonusPoolPlanStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BonusPoolPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"bonus pool plan"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Discount - Ratio, 2);
    }

    public BonusPoolPlan Copy()
    {
        return new BonusPoolPlan(Code, Name, Active, Rating, Segment, Ratio, Discount, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
