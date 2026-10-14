namespace Acme.Workforce.Assets.Warranty.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty tier is in its life cycle.</summary>
public enum WarrantyTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A warranty tier as the warranty team keeps it.</summary>
public sealed class WarrantyTier(string code, string label, double cost, bool active, string source, string badge, WarrantyTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public bool Active { get; set; } = active;
    public string Source { get; set; } = source;
    public string Badge { get; set; } = badge;
    public WarrantyTierStatus Status { get; set; } = status;

    /// <summary>Starts a warranty tier in status draft.</summary>
    public static WarrantyTier NewWarrantyTier(string code, string label)
    {
        return new WarrantyTier(code, label, 0.0, false, "", "", WarrantyTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyTierStatus next)
    {
        switch (Status)
        {
            case WarrantyTierStatus.Draft:
                switch (next)
                {
                    case WarrantyTierStatus.Active:
                        return true;
                }
                return false;
            case WarrantyTierStatus.Pending:
                switch (next)
                {
                    case WarrantyTierStatus.Closed:
                        return true;
                }
                return false;
            case WarrantyTierStatus.Active:
                switch (next)
                {
                    case WarrantyTierStatus.Suspended:
                    case WarrantyTierStatus.Closed:
                        return true;
                }
                return false;
            case WarrantyTierStatus.Suspended:
                switch (next)
                {
                    case WarrantyTierStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"warranty tier"} {Code}";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public WarrantyTier Copy()
    {
        return new WarrantyTier(Code, Label, Cost, Active, Source, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
