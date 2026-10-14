namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary certification bundle is in its life cycle.</summary>
public enum PrimaryCertificationBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a primary certification bundle and its current status.</summary>
public sealed class PrimaryCertificationBundle(string code, string name, int updatedAt, string country, int graceDays, bool pinned, string category, int quantity, int tenureMonths, string unit, PrimaryCertificationBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Country { get; set; } = country;
    public int GraceDays { get; set; } = graceDays;
    public bool Pinned { get; set; } = pinned;
    public string Category { get; set; } = category;
    public int Quantity { get; set; } = quantity;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Unit { get; set; } = unit;
    public PrimaryCertificationBundleStatus Status { get; set; } = status;

    /// <summary>Starts a primary certification bundle in status planned.</summary>
    public static PrimaryCertificationBundle NewPrimaryCertificationBundle(string code, string name)
    {
        return new PrimaryCertificationBundle(
            code,
            name,
            0,
            "",
            0,
            false,
            "",
            0,
            0,
            "",
            PrimaryCertificationBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryCertificationBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryCertificationBundleStatus next)
    {
        switch (Status)
        {
            case PrimaryCertificationBundleStatus.Planned:
                return next == PrimaryCertificationBundleStatus.Scheduled || next == PrimaryCertificationBundleStatus.Cancelled;
            case PrimaryCertificationBundleStatus.Scheduled:
                return next == PrimaryCertificationBundleStatus.InProgress;
            case PrimaryCertificationBundleStatus.InProgress:
                return next == PrimaryCertificationBundleStatus.Completed || next == PrimaryCertificationBundleStatus.Cancelled;
            case PrimaryCertificationBundleStatus.Completed:
                return next == PrimaryCertificationBundleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PrimaryCertificationBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary certification bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Quantity - (double)TenureMonths, 2);
    }

    public PrimaryCertificationBundle Copy()
    {
        return new PrimaryCertificationBundle(
            Code,
            Name,
            UpdatedAt,
            Country,
            GraceDays,
            Pinned,
            Category,
            Quantity,
            TenureMonths,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
