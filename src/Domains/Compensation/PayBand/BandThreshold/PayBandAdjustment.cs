namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band adjustment is in its life cycle.</summary>
public enum PayBandAdjustmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pay band adjustment. Its code is unique within a tenant.</summary>
public sealed class PayBandAdjustment(string code, string label, string externalRef, double fte, double rate, PayBandAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public double Fte { get; set; } = fte;
    public double Rate { get; set; } = rate;
    public PayBandAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a pay band adjustment in status requested.</summary>
    public static PayBandAdjustment NewPayBandAdjustment(string code, string label)
    {
        return new PayBandAdjustment(code, label, "", 0.0, 0.0, PayBandAdjustmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandAdjustmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandAdjustmentStatus next)
    {
        switch (Status)
        {
            case PayBandAdjustmentStatus.Requested:
                return next == PayBandAdjustmentStatus.Revoked;
            case PayBandAdjustmentStatus.Granted:
                switch (next)
                {
                    case PayBandAdjustmentStatus.Revoked:
                        return true;
                }
                return false;
            case PayBandAdjustmentStatus.Revoked:
                return next == PayBandAdjustmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PayBandAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Rate - Fte, 2);
    }

    public PayBandAdjustment Copy()
    {
        return new PayBandAdjustment(Code, Label, ExternalRef, Fte, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
