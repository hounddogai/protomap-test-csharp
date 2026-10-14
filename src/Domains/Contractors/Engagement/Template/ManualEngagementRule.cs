namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual engagement rule is in its life cycle.</summary>
public enum ManualEngagementRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One manual engagement rule, with what the planners need to act on it.</summary>
public sealed class ManualEngagementRule(string code, string label, int startsAt, double amount, int version, ManualEngagementRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public double Amount { get; set; } = amount;
    public int Version { get; set; } = version;
    public ManualEngagementRuleStatus Status { get; set; } = status;

    /// <summary>Starts a manual engagement rule in status proposed.</summary>
    public static ManualEngagementRule NewManualEngagementRule(string code, string label)
    {
        return new ManualEngagementRule(code, label, 0, 0.0, 0, ManualEngagementRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualEngagementRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualEngagementRuleStatus next)
    {
        switch (Status)
        {
            case ManualEngagementRuleStatus.Proposed:
                return next == ManualEngagementRuleStatus.Rejected || next == ManualEngagementRuleStatus.Withdrawn;
            case ManualEngagementRuleStatus.Approved:
                switch (next)
                {
                    case ManualEngagementRuleStatus.Rejected:
                    case ManualEngagementRuleStatus.Withdrawn:
                        return true;
                }
                return false;
            case ManualEngagementRuleStatus.Rejected:
                return next == ManualEngagementRuleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ManualEngagementRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual engagement rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Version - (double)StartsAt, 2);
    }

    public ManualEngagementRule Copy()
    {
        return new ManualEngagementRule(Code, Label, StartsAt, Amount, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
