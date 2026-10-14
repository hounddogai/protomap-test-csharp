namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive action plan slot is in its life cycle.</summary>
public enum AdaptiveActionPlanSlotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One adaptive action plan slot, with what the planners need to act on it.</summary>
public sealed class AdaptiveActionPlanSlot(string code, string title, string badge, string location, Dictionary<string, string> attributes, string ownerId, string employeeId, string externalRef, double score, AdaptiveActionPlanSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public string Location { get; set; } = location;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string OwnerId { get; set; } = ownerId;
    public string EmployeeId { get; set; } = employeeId;
    public string ExternalRef { get; set; } = externalRef;
    public double Score { get; set; } = score;
    public AdaptiveActionPlanSlotStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive action plan slot in status new.</summary>
    public static AdaptiveActionPlanSlot NewAdaptiveActionPlanSlot(string code, string title)
    {
        return new AdaptiveActionPlanSlot(
            code,
            title,
            "",
            "",
            new Dictionary<string, string>(),
            "",
            "",
            "",
            0.0,
            AdaptiveActionPlanSlotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveActionPlanSlotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveActionPlanSlotStatus next)
    {
        switch (Status)
        {
            case AdaptiveActionPlanSlotStatus.New:
                return next == AdaptiveActionPlanSlotStatus.Done || next == AdaptiveActionPlanSlotStatus.Failed;
            case AdaptiveActionPlanSlotStatus.Queued:
                switch (next)
                {
                    case AdaptiveActionPlanSlotStatus.Done:
                    case AdaptiveActionPlanSlotStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveActionPlanSlotStatus.Running:
                switch (next)
                {
                    case AdaptiveActionPlanSlotStatus.Done:
                    case AdaptiveActionPlanSlotStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveActionPlanSlotStatus.Done:
                switch (next)
                {
                    case AdaptiveActionPlanSlotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveActionPlanSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive action plan slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public AdaptiveActionPlanSlot Copy()
    {
        return new AdaptiveActionPlanSlot(
            Code,
            Title,
            Badge,
            Location,
            Attributes,
            OwnerId,
            EmployeeId,
            ExternalRef,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
