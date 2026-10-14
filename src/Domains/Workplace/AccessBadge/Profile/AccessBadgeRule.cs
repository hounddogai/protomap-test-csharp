namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge rule is in its life cycle.</summary>
public enum AccessBadgeRuleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A access badge rule as the access badge team keeps it.</summary>
public sealed class AccessBadgeRule(string code, string label, string channel, string phone, string ownerId, double score, AccessBadgeRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string Phone { get; set; } = phone;
    public string OwnerId { get; set; } = ownerId;
    public double Score { get; set; } = score;
    public AccessBadgeRuleStatus Status { get; set; } = status;

    /// <summary>Starts a access badge rule in status requested.</summary>
    public static AccessBadgeRule NewAccessBadgeRule(string code, string label)
    {
        return new AccessBadgeRule(code, label, "", "", "", 0.0, AccessBadgeRuleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeRuleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeRuleStatus next)
    {
        switch (Status)
        {
            case AccessBadgeRuleStatus.Requested:
                return next == AccessBadgeRuleStatus.Revoked;
            case AccessBadgeRuleStatus.Granted:
                switch (next)
                {
                    case AccessBadgeRuleStatus.Expired:
                        return true;
                }
                return false;
            case AccessBadgeRuleStatus.Revoked:
                switch (next)
                {
                    case AccessBadgeRuleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public AccessBadgeRule Copy()
    {
        return new AccessBadgeRule(Code, Label, Channel, Phone, OwnerId, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
