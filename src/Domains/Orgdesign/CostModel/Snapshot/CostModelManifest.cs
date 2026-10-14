namespace Acme.Workforce.Orgdesign.CostModel.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model manifest is in its life cycle.</summary>
public enum CostModelManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A cost model manifest. Its code is unique within a tenant.</summary>
public sealed class CostModelManifest(string code, string name, int expiresAt, string unit, int quantity, double discount, string source, string reviewer, string costCenter, CostModelManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Unit { get; set; } = unit;
    public int Quantity { get; set; } = quantity;
    public double Discount { get; set; } = discount;
    public string Source { get; set; } = source;
    public string Reviewer { get; set; } = reviewer;
    public string CostCenter { get; set; } = costCenter;
    public CostModelManifestStatus Status { get; set; } = status;

    /// <summary>Starts a cost model manifest in status created.</summary>
    public static CostModelManifest NewCostModelManifest(string code, string name)
    {
        return new CostModelManifest(code, name, 0, "", 0, 0.0, "", "", "", CostModelManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelManifestStatus next)
    {
        switch (Status)
        {
            case CostModelManifestStatus.Created:
                return next == CostModelManifestStatus.Published || next == CostModelManifestStatus.Retired;
            case CostModelManifestStatus.Validated:
                switch (next)
                {
                    case CostModelManifestStatus.Published:
                    case CostModelManifestStatus.Retired:
                        return true;
                }
                return false;
            case CostModelManifestStatus.Published:
                switch (next)
                {
                    case CostModelManifestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"cost model manifest"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Discount + (double)ExpiresAt, 2);
    }

    public CostModelManifest Copy()
    {
        return new CostModelManifest(
            Code,
            Name,
            ExpiresAt,
            Unit,
            Quantity,
            Discount,
            Source,
            Reviewer,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
