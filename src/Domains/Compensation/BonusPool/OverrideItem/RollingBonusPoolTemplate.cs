namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling bonus pool template is in its life cycle.</summary>
public enum RollingBonusPoolTemplateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A rolling bonus pool template as the bonus pool team keeps it.</summary>
public sealed class RollingBonusPoolTemplate(string code, string label, double weight, int durationMinutes, string reviewer, string unit, int attempts, double ratio, RollingBonusPoolTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Reviewer { get; set; } = reviewer;
    public string Unit { get; set; } = unit;
    public int Attempts { get; set; } = attempts;
    public double Ratio { get; set; } = ratio;
    public RollingBonusPoolTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a rolling bonus pool template in status new.</summary>
    public static RollingBonusPoolTemplate NewRollingBonusPoolTemplate(string code, string label)
    {
        return new RollingBonusPoolTemplate(code, label, 0.0, 0, "", "", 0, 0.0, RollingBonusPoolTemplateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingBonusPoolTemplateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingBonusPoolTemplateStatus next)
    {
        switch (Status)
        {
            case RollingBonusPoolTemplateStatus.New:
                return next == RollingBonusPoolTemplateStatus.Running || next == RollingBonusPoolTemplateStatus.Done;
            case RollingBonusPoolTemplateStatus.Queued:
                switch (next)
                {
                    case RollingBonusPoolTemplateStatus.Done:
                        return true;
                }
                return false;
            case RollingBonusPoolTemplateStatus.Running:
                return next == RollingBonusPoolTemplateStatus.Done || next == RollingBonusPoolTemplateStatus.Failed;
            case RollingBonusPoolTemplateStatus.Done:
                switch (next)
                {
                    case RollingBonusPoolTemplateStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingBonusPoolTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling bonus pool template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling bonus pool template"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Attempts * Weight, 2);
    }

    public RollingBonusPoolTemplate Copy()
    {
        return new RollingBonusPoolTemplate(
            Code,
            Label,
            Weight,
            DurationMinutes,
            Reviewer,
            Unit,
            Attempts,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
