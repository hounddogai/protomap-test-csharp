namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return override is in its life cycle.</summary>
public enum AssetReturnOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A asset return override. Its code is unique within a tenant.</summary>
public sealed class AssetReturnOverride(string code, string label, string grade, string source, int quantity, AssetReturnOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public string Source { get; set; } = source;
    public int Quantity { get; set; } = quantity;
    public AssetReturnOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a asset return override in status proposed.</summary>
    public static AssetReturnOverride NewAssetReturnOverride(string code, string label)
    {
        return new AssetReturnOverride(code, label, "", "", 0, AssetReturnOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnOverrideStatus next)
    {
        switch (Status)
        {
            case AssetReturnOverrideStatus.Proposed:
                switch (next)
                {
                    case AssetReturnOverrideStatus.Approved:
                    case AssetReturnOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
            case AssetReturnOverrideStatus.Approved:
                return next == AssetReturnOverrideStatus.Withdrawn;
            case AssetReturnOverrideStatus.Rejected:
                switch (next)
                {
                    case AssetReturnOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public AssetReturnOverride Copy()
    {
        return new AssetReturnOverride(Code, Label, Grade, Source, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
