namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return target is in its life cycle.</summary>
public enum AssetReturnTargetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A asset return target as the asset return team keeps it.</summary>
public sealed class AssetReturnTarget(string code, string label, string contactEmail, string timezone, string name, string currency, string phone, double amount, AssetReturnTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public string Timezone { get; set; } = timezone;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public string Phone { get; set; } = phone;
    public double Amount { get; set; } = amount;
    public AssetReturnTargetStatus Status { get; set; } = status;

    /// <summary>Starts a asset return target in status open.</summary>
    public static AssetReturnTarget NewAssetReturnTarget(string code, string label)
    {
        return new AssetReturnTarget(code, label, "", "", "", "", "", 0.0, AssetReturnTargetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnTargetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnTargetStatus next)
    {
        switch (Status)
        {
            case AssetReturnTargetStatus.Open:
                return next == AssetReturnTargetStatus.Reopened;
            case AssetReturnTargetStatus.InReview:
                return next == AssetReturnTargetStatus.Resolved;
            case AssetReturnTargetStatus.Resolved:
                switch (next)
                {
                    case AssetReturnTargetStatus.Reopened:
                    case AssetReturnTargetStatus.Archived:
                        return true;
                }
                return false;
            case AssetReturnTargetStatus.Reopened:
                return next == AssetReturnTargetStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AssetReturnTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"asset return target"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public AssetReturnTarget Copy()
    {
        return new AssetReturnTarget(Code, Label, ContactEmail, Timezone, Name, Currency, Phone, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
