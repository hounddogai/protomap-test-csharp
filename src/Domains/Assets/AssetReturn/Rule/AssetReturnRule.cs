namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return rule is in its life cycle.</summary>
public enum AssetReturnRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A asset return rule as the asset return team keeps it.</summary>
public sealed class AssetReturnRule(string code, string label, double ceiling, int graceDays, string department, double fte, string country, string region, AssetReturnRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public int GraceDays { get; set; } = graceDays;
    public string Department { get; set; } = department;
    public double Fte { get; set; } = fte;
    public string Country { get; set; } = country;
    public string Region { get; set; } = region;
    public AssetReturnRuleStatus Status { get; set; } = status;

    /// <summary>Starts a asset return rule in status idle.</summary>
    public static AssetReturnRule NewAssetReturnRule(string code, string label)
    {
        return new AssetReturnRule(code, label, 0.0, 0, "", 0.0, "", "", AssetReturnRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnRuleStatus next)
    {
        switch (Status)
        {
            case AssetReturnRuleStatus.Idle:
                return next == AssetReturnRuleStatus.Draining || next == AssetReturnRuleStatus.Stopped;
            case AssetReturnRuleStatus.Warming:
                switch (next)
                {
                    case AssetReturnRuleStatus.Stopped:
                        return true;
                }
                return false;
            case AssetReturnRuleStatus.Ready:
                switch (next)
                {
                    case AssetReturnRuleStatus.Draining:
                    case AssetReturnRuleStatus.Stopped:
                        return true;
                }
                return false;
            case AssetReturnRuleStatus.Draining:
                return next == AssetReturnRuleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AssetReturnRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Ceiling - (double)GraceDays, 2);
    }

    public AssetReturnRule Copy()
    {
        return new AssetReturnRule(Code, Label, Ceiling, GraceDays, Department, Fte, Country, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
