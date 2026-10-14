namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual action plan preference is in its life cycle.</summary>
public enum ManualActionPlanPreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One manual action plan preference, with what the planners need to act on it.</summary>
public sealed class ManualActionPlanPreference(string code, string name, int batchSize, Dictionary<string, string> attributes, string checksum, string department, double threshold, int quantity, double ratio, ManualActionPlanPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Checksum { get; set; } = checksum;
    public string Department { get; set; } = department;
    public double Threshold { get; set; } = threshold;
    public int Quantity { get; set; } = quantity;
    public double Ratio { get; set; } = ratio;
    public ManualActionPlanPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a manual action plan preference in status open.</summary>
    public static ManualActionPlanPreference NewManualActionPlanPreference(string code, string name)
    {
        return new ManualActionPlanPreference(
            code,
            name,
            0,
            new Dictionary<string, string>(),
            "",
            "",
            0.0,
            0,
            0.0,
            ManualActionPlanPreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualActionPlanPreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualActionPlanPreferenceStatus next)
    {
        switch (Status)
        {
            case ManualActionPlanPreferenceStatus.Open:
                switch (next)
                {
                    case ManualActionPlanPreferenceStatus.InReview:
                    case ManualActionPlanPreferenceStatus.Reopened:
                        return true;
                }
                return false;
            case ManualActionPlanPreferenceStatus.InReview:
                switch (next)
                {
                    case ManualActionPlanPreferenceStatus.Resolved:
                    case ManualActionPlanPreferenceStatus.Reopened:
                        return true;
                }
                return false;
            case ManualActionPlanPreferenceStatus.Resolved:
                switch (next)
                {
                    case ManualActionPlanPreferenceStatus.Reopened:
                    case ManualActionPlanPreferenceStatus.Archived:
                        return true;
                }
                return false;
            case ManualActionPlanPreferenceStatus.Reopened:
                return next == ManualActionPlanPreferenceStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ManualActionPlanPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual action plan preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"manual action plan preference"} {Code}";
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

    public double WeightedTotal()
    {
        return Math.Round(Ratio - Threshold, 2);
    }

    public ManualActionPlanPreference Copy()
    {
        return new ManualActionPlanPreference(
            Code,
            Name,
            BatchSize,
            Attributes,
            Checksum,
            Department,
            Threshold,
            Quantity,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
