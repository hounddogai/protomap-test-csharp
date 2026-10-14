namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional town hall bundle is in its life cycle.</summary>
public enum RegionalTownHallBundleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A regional town hall bundle. Its code is unique within a tenant.</summary>
public sealed class RegionalTownHallBundle(string code, string title, string externalRef, double floorValue, double threshold, RegionalTownHallBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public double FloorValue { get; set; } = floorValue;
    public double Threshold { get; set; } = threshold;
    public RegionalTownHallBundleStatus Status { get; set; } = status;

    /// <summary>Starts a regional town hall bundle in status proposed.</summary>
    public static RegionalTownHallBundle NewRegionalTownHallBundle(string code, string title)
    {
        return new RegionalTownHallBundle(code, title, "", 0.0, 0.0, RegionalTownHallBundleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalTownHallBundleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalTownHallBundleStatus next)
    {
        switch (Status)
        {
            case RegionalTownHallBundleStatus.Proposed:
                switch (next)
                {
                    case RegionalTownHallBundleStatus.Approved:
                    case RegionalTownHallBundleStatus.Withdrawn:
                        return true;
                }
                return false;
            case RegionalTownHallBundleStatus.Approved:
                switch (next)
                {
                    case RegionalTownHallBundleStatus.Rejected:
                        return true;
                }
                return false;
            case RegionalTownHallBundleStatus.Rejected:
                switch (next)
                {
                    case RegionalTownHallBundleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalTownHallBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional town hall bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Threshold - FloorValue, 2);
    }

    public RegionalTownHallBundle Copy()
    {
        return new RegionalTownHallBundle(Code, Title, ExternalRef, FloorValue, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
