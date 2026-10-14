namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan entry is in its life cycle.</summary>
public enum ActionPlanEntryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A action plan entry as the action plan team keeps it.</summary>
public sealed class ActionPlanEntry(string code, string label, string badge, string segment, double threshold, string notes, string managerId, int retries, List<string> tags, ActionPlanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public string Segment { get; set; } = segment;
    public double Threshold { get; set; } = threshold;
    public string Notes { get; set; } = notes;
    public string ManagerId { get; set; } = managerId;
    public int Retries { get; set; } = retries;
    public List<string> Tags { get; set; } = tags;
    public ActionPlanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a action plan entry in status created.</summary>
    public static ActionPlanEntry NewActionPlanEntry(string code, string label)
    {
        return new ActionPlanEntry(
            code,
            label,
            "",
            "",
            0.0,
            "",
            "",
            0,
            new List<string>(),
            ActionPlanEntryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanEntryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanEntryStatus next)
    {
        switch (Status)
        {
            case ActionPlanEntryStatus.Created:
                return next == ActionPlanEntryStatus.Validated || next == ActionPlanEntryStatus.Published;
            case ActionPlanEntryStatus.Validated:
                switch (next)
                {
                    case ActionPlanEntryStatus.Published:
                    case ActionPlanEntryStatus.Retired:
                        return true;
                }
                return false;
            case ActionPlanEntryStatus.Published:
                return next == ActionPlanEntryStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ActionPlanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"action plan entry"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedValue()
    {
        return Math.Round(Threshold - (double)Retries, 2);
    }

    public ActionPlanEntry Copy()
    {
        return new ActionPlanEntry(Code, Label, Badge, Segment, Threshold, Notes, ManagerId, Retries, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
