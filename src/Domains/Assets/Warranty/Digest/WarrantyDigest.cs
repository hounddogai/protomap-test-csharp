namespace Acme.Workforce.Assets.Warranty.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty digest is in its life cycle.</summary>
public enum WarrantyDigestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A warranty digest. Its code is unique within a tenant.</summary>
public sealed class WarrantyDigest(string code, string label, double score, int tenureMonths, string skill, string costCenter, double maximum, string badge, int graceDays, WarrantyDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Skill { get; set; } = skill;
    public string CostCenter { get; set; } = costCenter;
    public double Maximum { get; set; } = maximum;
    public string Badge { get; set; } = badge;
    public int GraceDays { get; set; } = graceDays;
    public WarrantyDigestStatus Status { get; set; } = status;

    /// <summary>Starts a warranty digest in status created.</summary>
    public static WarrantyDigest NewWarrantyDigest(string code, string label)
    {
        return new WarrantyDigest(code, label, 0.0, 0, "", "", 0.0, "", 0, WarrantyDigestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyDigestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyDigestStatus next)
    {
        switch (Status)
        {
            case WarrantyDigestStatus.Created:
                return next == WarrantyDigestStatus.Validated;
            case WarrantyDigestStatus.Validated:
                switch (next)
                {
                    case WarrantyDigestStatus.Published:
                        return true;
                }
                return false;
            case WarrantyDigestStatus.Published:
                return next == WarrantyDigestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WarrantyDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)TenureMonths * (double)GraceDays, 2);
    }

    public WarrantyDigest Copy()
    {
        return new WarrantyDigest(
            Code,
            Label,
            Score,
            TenureMonths,
            Skill,
            CostCenter,
            Maximum,
            Badge,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
