namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy target is in its life cycle.</summary>
public enum PolicyTargetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A policy target as the policy team keeps it.</summary>
public sealed class PolicyTarget(string code, string title, string category, string unit, double hours, string badge, double cost, string ownerId, Dictionary<string, string> attributes, double budget, string summary, PolicyTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string Unit { get; set; } = unit;
    public double Hours { get; set; } = hours;
    public string Badge { get; set; } = badge;
    public double Cost { get; set; } = cost;
    public string OwnerId { get; set; } = ownerId;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Budget { get; set; } = budget;
    public string Summary { get; set; } = summary;
    public PolicyTargetStatus Status { get; set; } = status;

    /// <summary>Starts a policy target in status created.</summary>
    public static PolicyTarget NewPolicyTarget(string code, string title)
    {
        return new PolicyTarget(
            code,
            title,
            "",
            "",
            0.0,
            "",
            0.0,
            "",
            new Dictionary<string, string>(),
            0.0,
            "",
            PolicyTargetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyTargetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyTargetStatus next)
    {
        switch (Status)
        {
            case PolicyTargetStatus.Created:
                switch (next)
                {
                    case PolicyTargetStatus.Retired:
                        return true;
                }
                return false;
            case PolicyTargetStatus.Validated:
                switch (next)
                {
                    case PolicyTargetStatus.Published:
                    case PolicyTargetStatus.Retired:
                        return true;
                }
                return false;
            case PolicyTargetStatus.Published:
                switch (next)
                {
                    case PolicyTargetStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"policy target"} {Code}";
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

    public double WeightedExposure()
    {
        return Math.Round(Budget + Hours, 2);
    }

    public PolicyTarget Copy()
    {
        return new PolicyTarget(
            Code,
            Title,
            Category,
            Unit,
            Hours,
            Badge,
            Cost,
            OwnerId,
            Attributes,
            Budget,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
