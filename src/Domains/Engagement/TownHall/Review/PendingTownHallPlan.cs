namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending town hall plan is in its life cycle.</summary>
public enum PendingTownHallPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One pending town hall plan, with what the planners need to act on it.</summary>
public sealed class PendingTownHallPlan(string code, string title, string name, double threshold, int graceDays, PendingTownHallPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public int GraceDays { get; set; } = graceDays;
    public PendingTownHallPlanStatus Status { get; set; } = status;

    /// <summary>Starts a pending town hall plan in status idle.</summary>
    public static PendingTownHallPlan NewPendingTownHallPlan(string code, string title)
    {
        return new PendingTownHallPlan(code, title, "", 0.0, 0, PendingTownHallPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingTownHallPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingTownHallPlanStatus next)
    {
        switch (Status)
        {
            case PendingTownHallPlanStatus.Idle:
                return next == PendingTownHallPlanStatus.Ready;
            case PendingTownHallPlanStatus.Warming:
                return next == PendingTownHallPlanStatus.Stopped;
            case PendingTownHallPlanStatus.Ready:
                switch (next)
                {
                    case PendingTownHallPlanStatus.Draining:
                    case PendingTownHallPlanStatus.Stopped:
                        return true;
                }
                return false;
            case PendingTownHallPlanStatus.Draining:
                switch (next)
                {
                    case PendingTownHallPlanStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingTownHallPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending town hall plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)GraceDays + Threshold, 2);
    }

    public PendingTownHallPlan Copy()
    {
        return new PendingTownHallPlan(Code, Title, Name, Threshold, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
