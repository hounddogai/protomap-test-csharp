namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive bonus pool baseline is in its life cycle.</summary>
public enum AdaptiveBonusPoolBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A adaptive bonus pool baseline as the bonus pool team keeps it.</summary>
public sealed class AdaptiveBonusPoolBaseline(string code, string label, bool visible, string skill, int capacity, AdaptiveBonusPoolBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public string Skill { get; set; } = skill;
    public int Capacity { get; set; } = capacity;
    public AdaptiveBonusPoolBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive bonus pool baseline in status planned.</summary>
    public static AdaptiveBonusPoolBaseline NewAdaptiveBonusPoolBaseline(string code, string label)
    {
        return new AdaptiveBonusPoolBaseline(code, label, false, "", 0, AdaptiveBonusPoolBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveBonusPoolBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveBonusPoolBaselineStatus next)
    {
        switch (Status)
        {
            case AdaptiveBonusPoolBaselineStatus.Planned:
                return next == AdaptiveBonusPoolBaselineStatus.Cancelled;
            case AdaptiveBonusPoolBaselineStatus.Scheduled:
                return next == AdaptiveBonusPoolBaselineStatus.Completed;
            case AdaptiveBonusPoolBaselineStatus.InProgress:
                switch (next)
                {
                    case AdaptiveBonusPoolBaselineStatus.Completed:
                    case AdaptiveBonusPoolBaselineStatus.Cancelled:
                        return true;
                }
                return false;
            case AdaptiveBonusPoolBaselineStatus.Completed:
                return next == AdaptiveBonusPoolBaselineStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AdaptiveBonusPoolBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive bonus pool baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adaptive bonus pool baseline"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Capacity;
    }

    public AdaptiveBonusPoolBaseline Copy()
    {
        return new AdaptiveBonusPoolBaseline(Code, Label, Visible, Skill, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
