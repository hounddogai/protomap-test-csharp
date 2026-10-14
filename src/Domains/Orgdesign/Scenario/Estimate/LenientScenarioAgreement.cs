namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient scenario agreement is in its life cycle.</summary>
public enum LenientScenarioAgreementStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One lenient scenario agreement, with what the planners need to act on it.</summary>
public sealed class LenientScenarioAgreement(string code, string label, bool visible, string summary, bool pinned, double amount, LenientScenarioAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public string Summary { get; set; } = summary;
    public bool Pinned { get; set; } = pinned;
    public double Amount { get; set; } = amount;
    public LenientScenarioAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a lenient scenario agreement in status proposed.</summary>
    public static LenientScenarioAgreement NewLenientScenarioAgreement(string code, string label)
    {
        return new LenientScenarioAgreement(
            code,
            label,
            false,
            "",
            false,
            0.0,
            LenientScenarioAgreementStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientScenarioAgreementStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientScenarioAgreementStatus next)
    {
        switch (Status)
        {
            case LenientScenarioAgreementStatus.Proposed:
                switch (next)
                {
                    case LenientScenarioAgreementStatus.Approved:
                    case LenientScenarioAgreementStatus.Rejected:
                        return true;
                }
                return false;
            case LenientScenarioAgreementStatus.Approved:
                return next == LenientScenarioAgreementStatus.Withdrawn;
            case LenientScenarioAgreementStatus.Rejected:
                return next == LenientScenarioAgreementStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LenientScenarioAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient scenario agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"lenient scenario agreement"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public LenientScenarioAgreement Copy()
    {
        return new LenientScenarioAgreement(Code, Label, Visible, Summary, Pinned, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
