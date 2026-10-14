namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return allocation is in its life cycle.</summary>
public enum AssetReturnAllocationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A asset return allocation as the asset return team keeps it.</summary>
public sealed class AssetReturnAllocation(string code, string name, double maximum, string title, string description, AssetReturnAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public AssetReturnAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a asset return allocation in status draft.</summary>
    public static AssetReturnAllocation NewAssetReturnAllocation(string code, string name)
    {
        return new AssetReturnAllocation(code, name, 0.0, "", "", AssetReturnAllocationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnAllocationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnAllocationStatus next)
    {
        switch (Status)
        {
            case AssetReturnAllocationStatus.Draft:
                return next == AssetReturnAllocationStatus.Pending || next == AssetReturnAllocationStatus.Closed;
            case AssetReturnAllocationStatus.Pending:
                return next == AssetReturnAllocationStatus.Active;
            case AssetReturnAllocationStatus.Active:
                return next == AssetReturnAllocationStatus.Suspended || next == AssetReturnAllocationStatus.Closed;
            case AssetReturnAllocationStatus.Suspended:
                switch (next)
                {
                    case AssetReturnAllocationStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public AssetReturnAllocation Copy()
    {
        return new AssetReturnAllocation(Code, Name, Maximum, Title, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
