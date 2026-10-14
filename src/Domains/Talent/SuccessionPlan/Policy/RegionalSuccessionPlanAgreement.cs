namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional succession plan agreement is in its life cycle.</summary>
public enum RegionalSuccessionPlanAgreementStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a regional succession plan agreement and its current status.</summary>
public sealed class RegionalSuccessionPlanAgreement(string code, string label, string location, string costCenter, double ratio, int sequence, string segment, RegionalSuccessionPlanAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public string CostCenter { get; set; } = costCenter;
    public double Ratio { get; set; } = ratio;
    public int Sequence { get; set; } = sequence;
    public string Segment { get; set; } = segment;
    public RegionalSuccessionPlanAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a regional succession plan agreement in status idle.</summary>
    public static RegionalSuccessionPlanAgreement NewRegionalSuccessionPlanAgreement(string code, string label)
    {
        return new RegionalSuccessionPlanAgreement(
            code,
            label,
            "",
            "",
            0.0,
            0,
            "",
            RegionalSuccessionPlanAgreementStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSuccessionPlanAgreementStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSuccessionPlanAgreementStatus next)
    {
        switch (Status)
        {
            case RegionalSuccessionPlanAgreementStatus.Idle:
                switch (next)
                {
                    case RegionalSuccessionPlanAgreementStatus.Draining:
                    case RegionalSuccessionPlanAgreementStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalSuccessionPlanAgreementStatus.Warming:
                return next == RegionalSuccessionPlanAgreementStatus.Ready;
            case RegionalSuccessionPlanAgreementStatus.Ready:
                switch (next)
                {
                    case RegionalSuccessionPlanAgreementStatus.Draining:
                    case RegionalSuccessionPlanAgreementStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalSuccessionPlanAgreementStatus.Draining:
                return next == RegionalSuccessionPlanAgreementStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RegionalSuccessionPlanAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional succession plan agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"regional succession plan agreement"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Sequence + Ratio, 2);
    }

    public RegionalSuccessionPlanAgreement Copy()
    {
        return new RegionalSuccessionPlanAgreement(
            Code,
            Label,
            Location,
            CostCenter,
            Ratio,
            Sequence,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
