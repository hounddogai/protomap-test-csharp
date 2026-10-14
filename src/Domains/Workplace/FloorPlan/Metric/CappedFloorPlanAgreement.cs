namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped floor plan agreement is in its life cycle.</summary>
public enum CappedFloorPlanAgreementStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One capped floor plan agreement, with what the planners need to act on it.</summary>
public sealed class CappedFloorPlanAgreement(string code, string name, string badge, bool pinned, double ratio, string currency, int quantity, double weight, CappedFloorPlanAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public bool Pinned { get; set; } = pinned;
    public double Ratio { get; set; } = ratio;
    public string Currency { get; set; } = currency;
    public int Quantity { get; set; } = quantity;
    public double Weight { get; set; } = weight;
    public CappedFloorPlanAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a capped floor plan agreement in status proposed.</summary>
    public static CappedFloorPlanAgreement NewCappedFloorPlanAgreement(string code, string name)
    {
        return new CappedFloorPlanAgreement(
            code,
            name,
            "",
            false,
            0.0,
            "",
            0,
            0.0,
            CappedFloorPlanAgreementStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedFloorPlanAgreementStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedFloorPlanAgreementStatus next)
    {
        switch (Status)
        {
            case CappedFloorPlanAgreementStatus.Proposed:
                return next == CappedFloorPlanAgreementStatus.Withdrawn;
            case CappedFloorPlanAgreementStatus.Approved:
                return next == CappedFloorPlanAgreementStatus.Rejected || next == CappedFloorPlanAgreementStatus.Withdrawn;
            case CappedFloorPlanAgreementStatus.Rejected:
                switch (next)
                {
                    case CappedFloorPlanAgreementStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedFloorPlanAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped floor plan agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped floor plan agreement"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Quantity + Weight, 2);
    }

    public CappedFloorPlanAgreement Copy()
    {
        return new CappedFloorPlanAgreement(Code, Name, Badge, Pinned, Ratio, Currency, Quantity, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
