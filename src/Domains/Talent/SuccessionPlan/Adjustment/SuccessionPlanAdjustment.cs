namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan adjustment is in its life cycle.</summary>
public enum SuccessionPlanAdjustmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a succession plan adjustment and its current status.</summary>
public sealed class SuccessionPlanAdjustment(string code, string title, string skill, string description, bool archived, int retries, double ratio, string badge, bool pinned, SuccessionPlanAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public string Description { get; set; } = description;
    public bool Archived { get; set; } = archived;
    public int Retries { get; set; } = retries;
    public double Ratio { get; set; } = ratio;
    public string Badge { get; set; } = badge;
    public bool Pinned { get; set; } = pinned;
    public SuccessionPlanAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan adjustment in status new.</summary>
    public static SuccessionPlanAdjustment NewSuccessionPlanAdjustment(string code, string title)
    {
        return new SuccessionPlanAdjustment(
            code,
            title,
            "",
            "",
            false,
            0,
            0.0,
            "",
            false,
            SuccessionPlanAdjustmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanAdjustmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanAdjustmentStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanAdjustmentStatus.New:
                switch (next)
                {
                    case SuccessionPlanAdjustmentStatus.Done:
                    case SuccessionPlanAdjustmentStatus.Failed:
                        return true;
                }
                return false;
            case SuccessionPlanAdjustmentStatus.Queued:
                return next == SuccessionPlanAdjustmentStatus.Running || next == SuccessionPlanAdjustmentStatus.Failed;
            case SuccessionPlanAdjustmentStatus.Running:
                switch (next)
                {
                    case SuccessionPlanAdjustmentStatus.Failed:
                        return true;
                }
                return false;
            case SuccessionPlanAdjustmentStatus.Done:
                switch (next)
                {
                    case SuccessionPlanAdjustmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Ratio + (double)Retries, 2);
    }

    public SuccessionPlanAdjustment Copy()
    {
        return new SuccessionPlanAdjustment(
            Code,
            Title,
            Skill,
            Description,
            Archived,
            Retries,
            Ratio,
            Badge,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
