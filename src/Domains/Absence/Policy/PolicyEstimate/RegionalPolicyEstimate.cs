namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional policy estimate is in its life cycle.</summary>
public enum RegionalPolicyEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One regional policy estimate, with what the planners need to act on it.</summary>
public sealed class RegionalPolicyEstimate(string code, string label, double threshold, int limit, double ceiling, bool visible, string costCenter, RegionalPolicyEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public int Limit { get; set; } = limit;
    public double Ceiling { get; set; } = ceiling;
    public bool Visible { get; set; } = visible;
    public string CostCenter { get; set; } = costCenter;
    public RegionalPolicyEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a regional policy estimate in status proposed.</summary>
    public static RegionalPolicyEstimate NewRegionalPolicyEstimate(string code, string label)
    {
        return new RegionalPolicyEstimate(
            code,
            label,
            0.0,
            0,
            0.0,
            false,
            "",
            RegionalPolicyEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPolicyEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPolicyEstimateStatus next)
    {
        switch (Status)
        {
            case RegionalPolicyEstimateStatus.Proposed:
                return next == RegionalPolicyEstimateStatus.Approved || next == RegionalPolicyEstimateStatus.Withdrawn;
            case RegionalPolicyEstimateStatus.Approved:
                return next == RegionalPolicyEstimateStatus.Rejected || next == RegionalPolicyEstimateStatus.Withdrawn;
            case RegionalPolicyEstimateStatus.Rejected:
                switch (next)
                {
                    case RegionalPolicyEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalPolicyEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional policy estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ceiling * Threshold, 2);
    }

    public RegionalPolicyEstimate Copy()
    {
        return new RegionalPolicyEstimate(Code, Label, Threshold, Limit, Ceiling, Visible, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
