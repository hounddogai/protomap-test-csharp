namespace Acme.Workforce.Contractors.Offboarding.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy offboarding bundle is in its life cycle.</summary>
public enum LegacyOffboardingBundleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A legacy offboarding bundle as the offboarding team keeps it.</summary>
public sealed class LegacyOffboardingBundle(string code, string label, double discount, double weight, double cost, string costCenter, LegacyOffboardingBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public double Weight { get; set; } = weight;
    public double Cost { get; set; } = cost;
    public string CostCenter { get; set; } = costCenter;
    public LegacyOffboardingBundleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy offboarding bundle in status open.</summary>
    public static LegacyOffboardingBundle NewLegacyOffboardingBundle(string code, string label)
    {
        return new LegacyOffboardingBundle(code, label, 0.0, 0.0, 0.0, "", LegacyOffboardingBundleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyOffboardingBundleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyOffboardingBundleStatus next)
    {
        switch (Status)
        {
            case LegacyOffboardingBundleStatus.Open:
                switch (next)
                {
                    case LegacyOffboardingBundleStatus.InReview:
                    case LegacyOffboardingBundleStatus.Resolved:
                        return true;
                }
                return false;
            case LegacyOffboardingBundleStatus.InReview:
                return next == LegacyOffboardingBundleStatus.Reopened;
            case LegacyOffboardingBundleStatus.Resolved:
                return next == LegacyOffboardingBundleStatus.Reopened;
            case LegacyOffboardingBundleStatus.Reopened:
                switch (next)
                {
                    case LegacyOffboardingBundleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyOffboardingBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy offboarding bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetVolume()
    {
        return Math.Round(Discount + Cost, 2);
    }

    public LegacyOffboardingBundle Copy()
    {
        return new LegacyOffboardingBundle(Code, Label, Discount, Weight, Cost, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
