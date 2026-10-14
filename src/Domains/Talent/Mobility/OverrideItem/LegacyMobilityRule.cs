namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy mobility rule is in its life cycle.</summary>
public enum LegacyMobilityRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One legacy mobility rule, with what the planners need to act on it.</summary>
public sealed class LegacyMobilityRule(string code, string title, string contactEmail, int attempts, string checksum, string label, int expiresAt, LegacyMobilityRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public int Attempts { get; set; } = attempts;
    public string Checksum { get; set; } = checksum;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public LegacyMobilityRuleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy mobility rule in status proposed.</summary>
    public static LegacyMobilityRule NewLegacyMobilityRule(string code, string title)
    {
        return new LegacyMobilityRule(code, title, "", 0, "", "", 0, LegacyMobilityRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyMobilityRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyMobilityRuleStatus next)
    {
        switch (Status)
        {
            case LegacyMobilityRuleStatus.Proposed:
                switch (next)
                {
                    case LegacyMobilityRuleStatus.Rejected:
                        return true;
                }
                return false;
            case LegacyMobilityRuleStatus.Approved:
                return next == LegacyMobilityRuleStatus.Withdrawn;
            case LegacyMobilityRuleStatus.Rejected:
                switch (next)
                {
                    case LegacyMobilityRuleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyMobilityRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy mobility rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"legacy mobility rule"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Attempts * (double)ExpiresAt, 2);
    }

    public LegacyMobilityRule Copy()
    {
        return new LegacyMobilityRule(Code, Title, ContactEmail, Attempts, Checksum, Label, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
