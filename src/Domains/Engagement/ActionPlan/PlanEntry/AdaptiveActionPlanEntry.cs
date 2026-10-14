namespace Acme.Workforce.Engagement.ActionPlan.PlanEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive action plan entry is in its life cycle.</summary>
public enum AdaptiveActionPlanEntryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One adaptive action plan entry, with what the planners need to act on it.</summary>
public sealed class AdaptiveActionPlanEntry(string code, string label, double rating, int capacity, double minimum, int version, string grade, double cost, AdaptiveActionPlanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public int Capacity { get; set; } = capacity;
    public double Minimum { get; set; } = minimum;
    public int Version { get; set; } = version;
    public string Grade { get; set; } = grade;
    public double Cost { get; set; } = cost;
    public AdaptiveActionPlanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive action plan entry in status idle.</summary>
    public static AdaptiveActionPlanEntry NewAdaptiveActionPlanEntry(string code, string label)
    {
        return new AdaptiveActionPlanEntry(code, label, 0.0, 0, 0.0, 0, "", 0.0, AdaptiveActionPlanEntryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveActionPlanEntryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveActionPlanEntryStatus next)
    {
        switch (Status)
        {
            case AdaptiveActionPlanEntryStatus.Idle:
                return next == AdaptiveActionPlanEntryStatus.Draining;
            case AdaptiveActionPlanEntryStatus.Warming:
                switch (next)
                {
                    case AdaptiveActionPlanEntryStatus.Draining:
                    case AdaptiveActionPlanEntryStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveActionPlanEntryStatus.Ready:
                switch (next)
                {
                    case AdaptiveActionPlanEntryStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveActionPlanEntryStatus.Draining:
                switch (next)
                {
                    case AdaptiveActionPlanEntryStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveActionPlanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive action plan entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Rating + Cost, 2);
    }

    public AdaptiveActionPlanEntry Copy()
    {
        return new AdaptiveActionPlanEntry(Code, Label, Rating, Capacity, Minimum, Version, Grade, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
