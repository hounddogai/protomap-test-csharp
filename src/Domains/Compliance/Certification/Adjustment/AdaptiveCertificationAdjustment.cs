namespace Acme.Workforce.Compliance.Certification.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive certification adjustment is in its life cycle.</summary>
public enum AdaptiveCertificationAdjustmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A adaptive certification adjustment as the certification team keeps it.</summary>
public sealed class AdaptiveCertificationAdjustment(string code, string name, string unit, double ceiling, string costCenter, AdaptiveCertificationAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public double Ceiling { get; set; } = ceiling;
    public string CostCenter { get; set; } = costCenter;
    public AdaptiveCertificationAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive certification adjustment in status created.</summary>
    public static AdaptiveCertificationAdjustment NewAdaptiveCertificationAdjustment(string code, string name)
    {
        return new AdaptiveCertificationAdjustment(
            code,
            name,
            "",
            0.0,
            "",
            AdaptiveCertificationAdjustmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveCertificationAdjustmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveCertificationAdjustmentStatus next)
    {
        switch (Status)
        {
            case AdaptiveCertificationAdjustmentStatus.Created:
                return next == AdaptiveCertificationAdjustmentStatus.Validated || next == AdaptiveCertificationAdjustmentStatus.Retired;
            case AdaptiveCertificationAdjustmentStatus.Validated:
                return next == AdaptiveCertificationAdjustmentStatus.Retired;
            case AdaptiveCertificationAdjustmentStatus.Published:
                return next == AdaptiveCertificationAdjustmentStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdaptiveCertificationAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive certification adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public AdaptiveCertificationAdjustment Copy()
    {
        return new AdaptiveCertificationAdjustment(Code, Name, Unit, Ceiling, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
