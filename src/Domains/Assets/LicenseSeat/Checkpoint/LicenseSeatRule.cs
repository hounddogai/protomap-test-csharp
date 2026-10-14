namespace Acme.Workforce.Assets.LicenseSeat.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat rule is in its life cycle.</summary>
public enum LicenseSeatRuleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One license seat rule, with what the planners need to act on it.</summary>
public sealed class LicenseSeatRule(string code, string title, string costCenter, string ownerId, bool pinned, string phone, double amount, double minimum, LicenseSeatRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public string OwnerId { get; set; } = ownerId;
    public bool Pinned { get; set; } = pinned;
    public string Phone { get; set; } = phone;
    public double Amount { get; set; } = amount;
    public double Minimum { get; set; } = minimum;
    public LicenseSeatRuleStatus Status { get; set; } = status;

    /// <summary>Starts a license seat rule in status created.</summary>
    public static LicenseSeatRule NewLicenseSeatRule(string code, string title)
    {
        return new LicenseSeatRule(code, title, "", "", false, "", 0.0, 0.0, LicenseSeatRuleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatRuleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatRuleStatus next)
    {
        switch (Status)
        {
            case LicenseSeatRuleStatus.Created:
                switch (next)
                {
                    case LicenseSeatRuleStatus.Retired:
                        return true;
                }
                return false;
            case LicenseSeatRuleStatus.Validated:
                return next == LicenseSeatRuleStatus.Published;
            case LicenseSeatRuleStatus.Published:
                switch (next)
                {
                    case LicenseSeatRuleStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Minimum - Amount, 2);
    }

    public LicenseSeatRule Copy()
    {
        return new LicenseSeatRule(Code, Title, CostCenter, OwnerId, Pinned, Phone, Amount, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
