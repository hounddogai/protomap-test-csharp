namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered headcount claim is in its life cycle.</summary>
public enum TieredHeadcountClaimStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One tiered headcount claim, with what the planners need to act on it.</summary>
public sealed class TieredHeadcountClaim(string code, string label, string unit, double margin, string country, TieredHeadcountClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public double Margin { get; set; } = margin;
    public string Country { get; set; } = country;
    public TieredHeadcountClaimStatus Status { get; set; } = status;

    /// <summary>Starts a tiered headcount claim in status proposed.</summary>
    public static TieredHeadcountClaim NewTieredHeadcountClaim(string code, string label)
    {
        return new TieredHeadcountClaim(code, label, "", 0.0, "", TieredHeadcountClaimStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredHeadcountClaimStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredHeadcountClaimStatus next)
    {
        switch (Status)
        {
            case TieredHeadcountClaimStatus.Proposed:
                switch (next)
                {
                    case TieredHeadcountClaimStatus.Approved:
                    case TieredHeadcountClaimStatus.Rejected:
                        return true;
                }
                return false;
            case TieredHeadcountClaimStatus.Approved:
                return next == TieredHeadcountClaimStatus.Withdrawn;
            case TieredHeadcountClaimStatus.Rejected:
                switch (next)
                {
                    case TieredHeadcountClaimStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredHeadcountClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered headcount claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Margin;
    }

    public TieredHeadcountClaim Copy()
    {
        return new TieredHeadcountClaim(Code, Label, Unit, Margin, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
