namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment milestone is in its life cycle.</summary>
public enum AdjustmentMilestoneStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A adjustment milestone. Its code is unique within a tenant.</summary>
public sealed class AdjustmentMilestone(string code, string label, double fte, double ratio, int attempts, double threshold, AdjustmentMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public double Ratio { get; set; } = ratio;
    public int Attempts { get; set; } = attempts;
    public double Threshold { get; set; } = threshold;
    public AdjustmentMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment milestone in status new.</summary>
    public static AdjustmentMilestone NewAdjustmentMilestone(string code, string label)
    {
        return new AdjustmentMilestone(code, label, 0.0, 0.0, 0, 0.0, AdjustmentMilestoneStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentMilestoneStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentMilestoneStatus next)
    {
        switch (Status)
        {
            case AdjustmentMilestoneStatus.New:
                switch (next)
                {
                    case AdjustmentMilestoneStatus.Queued:
                    case AdjustmentMilestoneStatus.Running:
                        return true;
                }
                return false;
            case AdjustmentMilestoneStatus.Queued:
                switch (next)
                {
                    case AdjustmentMilestoneStatus.Failed:
                        return true;
                }
                return false;
            case AdjustmentMilestoneStatus.Running:
                switch (next)
                {
                    case AdjustmentMilestoneStatus.Done:
                        return true;
                }
                return false;
            case AdjustmentMilestoneStatus.Done:
                return next == AdjustmentMilestoneStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdjustmentMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetLoad()
    {
        return Math.Round(Fte + (double)Attempts, 2);
    }

    public AdjustmentMilestone Copy()
    {
        return new AdjustmentMilestone(Code, Label, Fte, Ratio, Attempts, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
