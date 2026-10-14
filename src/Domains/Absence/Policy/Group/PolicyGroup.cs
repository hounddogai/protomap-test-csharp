namespace Acme.Workforce.Absence.Policy.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy group is in its life cycle.</summary>
public enum PolicyGroupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One policy group, with what the planners need to act on it.</summary>
public sealed class PolicyGroup(string code, string name, double score, int graceDays, string category, string reviewer, string source, string currency, string label, List<string> tags, PolicyGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public int GraceDays { get; set; } = graceDays;
    public string Category { get; set; } = category;
    public string Reviewer { get; set; } = reviewer;
    public string Source { get; set; } = source;
    public string Currency { get; set; } = currency;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public PolicyGroupStatus Status { get; set; } = status;

    /// <summary>Starts a policy group in status open.</summary>
    public static PolicyGroup NewPolicyGroup(string code, string name)
    {
        return new PolicyGroup(code, name, 0.0, 0, "", "", "", "", "", new List<string>(), PolicyGroupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyGroupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyGroupStatus next)
    {
        switch (Status)
        {
            case PolicyGroupStatus.Open:
                switch (next)
                {
                    case PolicyGroupStatus.Reopened:
                        return true;
                }
                return false;
            case PolicyGroupStatus.InReview:
                return next == PolicyGroupStatus.Archived;
            case PolicyGroupStatus.Resolved:
                switch (next)
                {
                    case PolicyGroupStatus.Archived:
                        return true;
                }
                return false;
            case PolicyGroupStatus.Reopened:
                switch (next)
                {
                    case PolicyGroupStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"policy group"} {Code}";
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
        return Math.Round((double)GraceDays - Score, 2);
    }

    public PolicyGroup Copy()
    {
        return new PolicyGroup(
            Code,
            Name,
            Score,
            GraceDays,
            Category,
            Reviewer,
            Source,
            Currency,
            Label,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
