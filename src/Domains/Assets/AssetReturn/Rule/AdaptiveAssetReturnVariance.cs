namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive asset return variance is in its life cycle.</summary>
public enum AdaptiveAssetReturnVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A adaptive asset return variance as the asset return team keeps it.</summary>
public sealed class AdaptiveAssetReturnVariance(string code, string name, int leaveDays, double cost, int createdAt, int quantity, string description, double ceiling, string salaryBand, bool archived, AdaptiveAssetReturnVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public double Cost { get; set; } = cost;
    public int CreatedAt { get; set; } = createdAt;
    public int Quantity { get; set; } = quantity;
    public string Description { get; set; } = description;
    public double Ceiling { get; set; } = ceiling;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Archived { get; set; } = archived;
    public AdaptiveAssetReturnVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive asset return variance in status draft.</summary>
    public static AdaptiveAssetReturnVariance NewAdaptiveAssetReturnVariance(string code, string name)
    {
        return new AdaptiveAssetReturnVariance(
            code,
            name,
            0,
            0.0,
            0,
            0,
            "",
            0.0,
            "",
            false,
            AdaptiveAssetReturnVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAssetReturnVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAssetReturnVarianceStatus next)
    {
        switch (Status)
        {
            case AdaptiveAssetReturnVarianceStatus.Draft:
                return next == AdaptiveAssetReturnVarianceStatus.Suspended || next == AdaptiveAssetReturnVarianceStatus.Closed;
            case AdaptiveAssetReturnVarianceStatus.Pending:
                switch (next)
                {
                    case AdaptiveAssetReturnVarianceStatus.Closed:
                        return true;
                }
                return false;
            case AdaptiveAssetReturnVarianceStatus.Active:
                switch (next)
                {
                    case AdaptiveAssetReturnVarianceStatus.Suspended:
                    case AdaptiveAssetReturnVarianceStatus.Closed:
                        return true;
                }
                return false;
            case AdaptiveAssetReturnVarianceStatus.Suspended:
                switch (next)
                {
                    case AdaptiveAssetReturnVarianceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveAssetReturnVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive asset return variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Quantity + (double)CreatedAt, 2);
    }

    public AdaptiveAssetReturnVariance Copy()
    {
        return new AdaptiveAssetReturnVariance(
            Code,
            Name,
            LeaveDays,
            Cost,
            CreatedAt,
            Quantity,
            Description,
            Ceiling,
            SalaryBand,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
