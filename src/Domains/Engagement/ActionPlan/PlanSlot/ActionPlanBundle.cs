namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan bundle is in its life cycle.</summary>
public enum ActionPlanBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A action plan bundle as the action plan team keeps it.</summary>
public sealed class ActionPlanBundle(string code, string name, string description, double threshold, string contactEmail, string phone, int capacity, Dictionary<string, string> attributes, string region, ActionPlanBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public double Threshold { get; set; } = threshold;
    public string ContactEmail { get; set; } = contactEmail;
    public string Phone { get; set; } = phone;
    public int Capacity { get; set; } = capacity;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Region { get; set; } = region;
    public ActionPlanBundleStatus Status { get; set; } = status;

    /// <summary>Starts a action plan bundle in status new.</summary>
    public static ActionPlanBundle NewActionPlanBundle(string code, string name)
    {
        return new ActionPlanBundle(
            code,
            name,
            "",
            0.0,
            "",
            "",
            0,
            new Dictionary<string, string>(),
            "",
            ActionPlanBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanBundleStatus next)
    {
        switch (Status)
        {
            case ActionPlanBundleStatus.New:
                return next == ActionPlanBundleStatus.Running;
            case ActionPlanBundleStatus.Queued:
                switch (next)
                {
                    case ActionPlanBundleStatus.Running:
                    case ActionPlanBundleStatus.Failed:
                        return true;
                }
                return false;
            case ActionPlanBundleStatus.Running:
                switch (next)
                {
                    case ActionPlanBundleStatus.Done:
                    case ActionPlanBundleStatus.Failed:
                        return true;
                }
                return false;
            case ActionPlanBundleStatus.Done:
                switch (next)
                {
                    case ActionPlanBundleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double WeightedVolume()
    {
        return Math.Round(Threshold + (double)Capacity, 2);
    }

    public ActionPlanBundle Copy()
    {
        return new ActionPlanBundle(
            Code,
            Name,
            Description,
            Threshold,
            ContactEmail,
            Phone,
            Capacity,
            Attributes,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
