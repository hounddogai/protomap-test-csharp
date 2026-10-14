namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict asset return tier is in its life cycle.</summary>
public enum StrictAssetReturnTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One strict asset return tier, with what the planners need to act on it.</summary>
public sealed class StrictAssetReturnTier(string code, string label, string externalRef, bool approved, int leaveDays, string location, string name, double budget, string jobCode, StrictAssetReturnTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public bool Approved { get; set; } = approved;
    public int LeaveDays { get; set; } = leaveDays;
    public string Location { get; set; } = location;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public string JobCode { get; set; } = jobCode;
    public StrictAssetReturnTierStatus Status { get; set; } = status;

    /// <summary>Starts a strict asset return tier in status draft.</summary>
    public static StrictAssetReturnTier NewStrictAssetReturnTier(string code, string label)
    {
        return new StrictAssetReturnTier(
            code,
            label,
            "",
            false,
            0,
            "",
            "",
            0.0,
            "",
            StrictAssetReturnTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAssetReturnTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAssetReturnTierStatus next)
    {
        switch (Status)
        {
            case StrictAssetReturnTierStatus.Draft:
                switch (next)
                {
                    case StrictAssetReturnTierStatus.Pending:
                    case StrictAssetReturnTierStatus.Closed:
                        return true;
                }
                return false;
            case StrictAssetReturnTierStatus.Pending:
                switch (next)
                {
                    case StrictAssetReturnTierStatus.Suspended:
                        return true;
                }
                return false;
            case StrictAssetReturnTierStatus.Active:
                switch (next)
                {
                    case StrictAssetReturnTierStatus.Suspended:
                        return true;
                }
                return false;
            case StrictAssetReturnTierStatus.Suspended:
                return next == StrictAssetReturnTierStatus.Closed;
        }
        return false;
    }

    public void MoveTo(StrictAssetReturnTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict asset return tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Budget - (double)LeaveDays, 2);
    }

    public StrictAssetReturnTier Copy()
    {
        return new StrictAssetReturnTier(
            Code,
            Label,
            ExternalRef,
            Approved,
            LeaveDays,
            Location,
            Name,
            Budget,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
