namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered tax form estimate is in its life cycle.</summary>
public enum TieredTaxFormEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One tiered tax form estimate, with what the planners need to act on it.</summary>
public sealed class TieredTaxFormEstimate(string code, string name, string notes, string externalRef, double ratio, TieredTaxFormEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public string ExternalRef { get; set; } = externalRef;
    public double Ratio { get; set; } = ratio;
    public TieredTaxFormEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a tiered tax form estimate in status proposed.</summary>
    public static TieredTaxFormEstimate NewTieredTaxFormEstimate(string code, string name)
    {
        return new TieredTaxFormEstimate(code, name, "", "", 0.0, TieredTaxFormEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredTaxFormEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredTaxFormEstimateStatus next)
    {
        switch (Status)
        {
            case TieredTaxFormEstimateStatus.Proposed:
                switch (next)
                {
                    case TieredTaxFormEstimateStatus.Approved:
                    case TieredTaxFormEstimateStatus.Rejected:
                        return true;
                }
                return false;
            case TieredTaxFormEstimateStatus.Approved:
                switch (next)
                {
                    case TieredTaxFormEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
            case TieredTaxFormEstimateStatus.Rejected:
                switch (next)
                {
                    case TieredTaxFormEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredTaxFormEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered tax form estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public TieredTaxFormEstimate Copy()
    {
        return new TieredTaxFormEstimate(Code, Name, Notes, ExternalRef, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
