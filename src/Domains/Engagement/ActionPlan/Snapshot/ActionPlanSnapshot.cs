namespace Acme.Workforce.Engagement.ActionPlan.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan snapshot is in its life cycle.</summary>
public enum ActionPlanSnapshotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One action plan snapshot, with what the planners need to act on it.</summary>
public sealed class ActionPlanSnapshot(string code, string label, string badge, int windowDays, double rating, Dictionary<string, string> attributes, List<string> tags, string locale, double weight, int expiresAt, int version, ActionPlanSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public int WindowDays { get; set; } = windowDays;
    public double Rating { get; set; } = rating;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public List<string> Tags { get; set; } = tags;
    public string Locale { get; set; } = locale;
    public double Weight { get; set; } = weight;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Version { get; set; } = version;
    public ActionPlanSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a action plan snapshot in status requested.</summary>
    public static ActionPlanSnapshot NewActionPlanSnapshot(string code, string label)
    {
        return new ActionPlanSnapshot(
            code,
            label,
            "",
            0,
            0.0,
            new Dictionary<string, string>(),
            new List<string>(),
            "",
            0.0,
            0,
            0,
            ActionPlanSnapshotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanSnapshotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanSnapshotStatus next)
    {
        switch (Status)
        {
            case ActionPlanSnapshotStatus.Requested:
                return next == ActionPlanSnapshotStatus.Revoked || next == ActionPlanSnapshotStatus.Expired;
            case ActionPlanSnapshotStatus.Granted:
                switch (next)
                {
                    case ActionPlanSnapshotStatus.Expired:
                        return true;
                }
                return false;
            case ActionPlanSnapshotStatus.Revoked:
                switch (next)
                {
                    case ActionPlanSnapshotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"action plan snapshot"} {Code}";
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

    public double AdjustedExposure()
    {
        return Math.Round((double)Version - (double)ExpiresAt, 2);
    }

    public ActionPlanSnapshot Copy()
    {
        return new ActionPlanSnapshot(
            Code,
            Label,
            Badge,
            WindowDays,
            Rating,
            Attributes,
            Tags,
            Locale,
            Weight,
            ExpiresAt,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
