namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return quota is in its life cycle.</summary>
public enum AssetReturnQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A asset return quota as the asset return team keeps it.</summary>
public sealed class AssetReturnQuota(string code, string name, string title, double maximum, string contactEmail, AssetReturnQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public string ContactEmail { get; set; } = contactEmail;
    public AssetReturnQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a asset return quota in status planned.</summary>
    public static AssetReturnQuota NewAssetReturnQuota(string code, string name)
    {
        return new AssetReturnQuota(code, name, "", 0.0, "", AssetReturnQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnQuotaStatus next)
    {
        switch (Status)
        {
            case AssetReturnQuotaStatus.Planned:
                switch (next)
                {
                    case AssetReturnQuotaStatus.Scheduled:
                    case AssetReturnQuotaStatus.Completed:
                        return true;
                }
                return false;
            case AssetReturnQuotaStatus.Scheduled:
                switch (next)
                {
                    case AssetReturnQuotaStatus.Cancelled:
                        return true;
                }
                return false;
            case AssetReturnQuotaStatus.InProgress:
                switch (next)
                {
                    case AssetReturnQuotaStatus.Completed:
                    case AssetReturnQuotaStatus.Cancelled:
                        return true;
                }
                return false;
            case AssetReturnQuotaStatus.Completed:
                return next == AssetReturnQuotaStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AssetReturnQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public AssetReturnQuota Copy()
    {
        return new AssetReturnQuota(Code, Name, Title, Maximum, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
