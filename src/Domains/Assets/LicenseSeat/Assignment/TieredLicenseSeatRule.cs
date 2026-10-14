namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered license seat rule is in its life cycle.</summary>
public enum TieredLicenseSeatRuleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A tiered license seat rule. Its code is unique within a tenant.</summary>
public sealed class TieredLicenseSeatRule(string code, string label, int limit, int startsAt, bool approved, bool active, double ratio, string phone, string location, double floorValue, TieredLicenseSeatRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public int StartsAt { get; set; } = startsAt;
    public bool Approved { get; set; } = approved;
    public bool Active { get; set; } = active;
    public double Ratio { get; set; } = ratio;
    public string Phone { get; set; } = phone;
    public string Location { get; set; } = location;
    public double FloorValue { get; set; } = floorValue;
    public TieredLicenseSeatRuleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered license seat rule in status open.</summary>
    public static TieredLicenseSeatRule NewTieredLicenseSeatRule(string code, string label)
    {
        return new TieredLicenseSeatRule(
            code,
            label,
            0,
            0,
            false,
            false,
            0.0,
            "",
            "",
            0.0,
            TieredLicenseSeatRuleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredLicenseSeatRuleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredLicenseSeatRuleStatus next)
    {
        switch (Status)
        {
            case TieredLicenseSeatRuleStatus.Open:
                return next == TieredLicenseSeatRuleStatus.Resolved || next == TieredLicenseSeatRuleStatus.Archived;
            case TieredLicenseSeatRuleStatus.InReview:
                return next == TieredLicenseSeatRuleStatus.Reopened || next == TieredLicenseSeatRuleStatus.Archived;
            case TieredLicenseSeatRuleStatus.Resolved:
                return next == TieredLicenseSeatRuleStatus.Archived;
            case TieredLicenseSeatRuleStatus.Reopened:
                return next == TieredLicenseSeatRuleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(TieredLicenseSeatRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered license seat rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetExposure()
    {
        return Math.Round(FloorValue - Ratio, 2);
    }

    public TieredLicenseSeatRule Copy()
    {
        return new TieredLicenseSeatRule(
            Code,
            Label,
            Limit,
            StartsAt,
            Approved,
            Active,
            Ratio,
            Phone,
            Location,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
