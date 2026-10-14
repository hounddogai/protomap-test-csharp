namespace Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary scenario agreement is in its life cycle.</summary>
public enum PrimaryScenarioAgreementStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A primary scenario agreement as the scenario team keeps it.</summary>
public sealed class PrimaryScenarioAgreement(string code, string label, double minimum, string team, bool approved, string name, int quantity, PrimaryScenarioAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public string Team { get; set; } = team;
    public bool Approved { get; set; } = approved;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public PrimaryScenarioAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a primary scenario agreement in status requested.</summary>
    public static PrimaryScenarioAgreement NewPrimaryScenarioAgreement(string code, string label)
    {
        return new PrimaryScenarioAgreement(
            code,
            label,
            0.0,
            "",
            false,
            "",
            0,
            PrimaryScenarioAgreementStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryScenarioAgreementStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryScenarioAgreementStatus next)
    {
        switch (Status)
        {
            case PrimaryScenarioAgreementStatus.Requested:
                return next == PrimaryScenarioAgreementStatus.Granted || next == PrimaryScenarioAgreementStatus.Expired;
            case PrimaryScenarioAgreementStatus.Granted:
                switch (next)
                {
                    case PrimaryScenarioAgreementStatus.Revoked:
                    case PrimaryScenarioAgreementStatus.Expired:
                        return true;
                }
                return false;
            case PrimaryScenarioAgreementStatus.Revoked:
                return next == PrimaryScenarioAgreementStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PrimaryScenarioAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary scenario agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Quantity - Minimum, 2);
    }

    public PrimaryScenarioAgreement Copy()
    {
        return new PrimaryScenarioAgreement(Code, Label, Minimum, Team, Approved, Name, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
