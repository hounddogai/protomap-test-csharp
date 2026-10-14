namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy item is in its life cycle.</summary>
public enum PolicyItemStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A policy item as the policy team keeps it.</summary>
public sealed class PolicyItem(string code, string label, string currency, bool visible, string team, string locale, string category, double discount, int updatedAt, List<string> tags, string externalRef, PolicyItemStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public bool Visible { get; set; } = visible;
    public string Team { get; set; } = team;
    public string Locale { get; set; } = locale;
    public string Category { get; set; } = category;
    public double Discount { get; set; } = discount;
    public int UpdatedAt { get; set; } = updatedAt;
    public List<string> Tags { get; set; } = tags;
    public string ExternalRef { get; set; } = externalRef;
    public PolicyItemStatus Status { get; set; } = status;

    /// <summary>Starts a policy item in status created.</summary>
    public static PolicyItem NewPolicyItem(string code, string label)
    {
        return new PolicyItem(
            code,
            label,
            "",
            false,
            "",
            "",
            "",
            0.0,
            0,
            new List<string>(),
            "",
            PolicyItemStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyItemStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyItemStatus next)
    {
        switch (Status)
        {
            case PolicyItemStatus.Created:
                return next == PolicyItemStatus.Validated;
            case PolicyItemStatus.Validated:
                switch (next)
                {
                    case PolicyItemStatus.Published:
                    case PolicyItemStatus.Retired:
                        return true;
                }
                return false;
            case PolicyItemStatus.Published:
                return next == PolicyItemStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PolicyItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"policy item"} {Code}";
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

    public double EffectiveValue()
    {
        return Math.Round((double)UpdatedAt * Discount, 2);
    }

    public PolicyItem Copy()
    {
        return new PolicyItem(
            Code,
            Label,
            Currency,
            Visible,
            Team,
            Locale,
            Category,
            Discount,
            UpdatedAt,
            Tags,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
