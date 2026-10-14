namespace Acme.Workforce.Assets.Assignment.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional assignment rule is in its life cycle.</summary>
public enum RegionalAssignmentRuleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a regional assignment rule and its current status.</summary>
public sealed class RegionalAssignmentRule(string code, string label, int startsAt, double margin, int expiresAt, string summary, double weight, int leaveDays, bool visible, double rate, RegionalAssignmentRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public double Margin { get; set; } = margin;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Summary { get; set; } = summary;
    public double Weight { get; set; } = weight;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Visible { get; set; } = visible;
    public double Rate { get; set; } = rate;
    public RegionalAssignmentRuleStatus Status { get; set; } = status;

    /// <summary>Starts a regional assignment rule in status draft.</summary>
    public static RegionalAssignmentRule NewRegionalAssignmentRule(string code, string label)
    {
        return new RegionalAssignmentRule(
            code,
            label,
            0,
            0.0,
            0,
            "",
            0.0,
            0,
            false,
            0.0,
            RegionalAssignmentRuleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAssignmentRuleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAssignmentRuleStatus next)
    {
        switch (Status)
        {
            case RegionalAssignmentRuleStatus.Draft:
                switch (next)
                {
                    case RegionalAssignmentRuleStatus.Closed:
                        return true;
                }
                return false;
            case RegionalAssignmentRuleStatus.Pending:
                return next == RegionalAssignmentRuleStatus.Closed;
            case RegionalAssignmentRuleStatus.Active:
                return next == RegionalAssignmentRuleStatus.Suspended || next == RegionalAssignmentRuleStatus.Closed;
            case RegionalAssignmentRuleStatus.Suspended:
                switch (next)
                {
                    case RegionalAssignmentRuleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalAssignmentRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional assignment rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)LeaveDays * (double)ExpiresAt, 2);
    }

    public RegionalAssignmentRule Copy()
    {
        return new RegionalAssignmentRule(
            Code,
            Label,
            StartsAt,
            Margin,
            ExpiresAt,
            Summary,
            Weight,
            LeaveDays,
            Visible,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
