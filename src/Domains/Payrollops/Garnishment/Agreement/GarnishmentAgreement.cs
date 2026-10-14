namespace Acme.Workforce.Payrollops.Garnishment.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment agreement is in its life cycle.</summary>
public enum GarnishmentAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a garnishment agreement and its current status.</summary>
public sealed class GarnishmentAgreement(string code, string title, bool approved, double floorValue, string name, double minimum, GarnishmentAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public double FloorValue { get; set; } = floorValue;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public GarnishmentAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment agreement in status created.</summary>
    public static GarnishmentAgreement NewGarnishmentAgreement(string code, string title)
    {
        return new GarnishmentAgreement(code, title, false, 0.0, "", 0.0, GarnishmentAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentAgreementStatus next)
    {
        switch (Status)
        {
            case GarnishmentAgreementStatus.Created:
                return next == GarnishmentAgreementStatus.Retired;
            case GarnishmentAgreementStatus.Validated:
                switch (next)
                {
                    case GarnishmentAgreementStatus.Published:
                    case GarnishmentAgreementStatus.Retired:
                        return true;
                }
                return false;
            case GarnishmentAgreementStatus.Published:
                switch (next)
                {
                    case GarnishmentAgreementStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveValue()
    {
        return Math.Round(Minimum - FloorValue, 2);
    }

    public GarnishmentAgreement Copy()
    {
        return new GarnishmentAgreement(Code, Title, Approved, FloorValue, Name, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
