namespace Acme.Workforce.Absence.Policy.PolicyRollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive policy override is in its life cycle.</summary>
public enum AdaptivePolicyOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A adaptive policy override as the policy team keeps it.</summary>
public sealed class AdaptivePolicyOverride(string code, string name, bool active, string team, bool archived, double margin, string checksum, string costCenter, AdaptivePolicyOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public string Team { get; set; } = team;
    public bool Archived { get; set; } = archived;
    public double Margin { get; set; } = margin;
    public string Checksum { get; set; } = checksum;
    public string CostCenter { get; set; } = costCenter;
    public AdaptivePolicyOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive policy override in status open.</summary>
    public static AdaptivePolicyOverride NewAdaptivePolicyOverride(string code, string name)
    {
        return new AdaptivePolicyOverride(
            code,
            name,
            false,
            "",
            false,
            0.0,
            "",
            "",
            AdaptivePolicyOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePolicyOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePolicyOverrideStatus next)
    {
        switch (Status)
        {
            case AdaptivePolicyOverrideStatus.Open:
                return next == AdaptivePolicyOverrideStatus.InReview;
            case AdaptivePolicyOverrideStatus.InReview:
                return next == AdaptivePolicyOverrideStatus.Resolved || next == AdaptivePolicyOverrideStatus.Reopened;
            case AdaptivePolicyOverrideStatus.Resolved:
                return next == AdaptivePolicyOverrideStatus.Reopened || next == AdaptivePolicyOverrideStatus.Archived;
            case AdaptivePolicyOverrideStatus.Reopened:
                return next == AdaptivePolicyOverrideStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptivePolicyOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive policy override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Margin;
    }

    public AdaptivePolicyOverride Copy()
    {
        return new AdaptivePolicyOverride(Code, Name, Active, Team, Archived, Margin, Checksum, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
