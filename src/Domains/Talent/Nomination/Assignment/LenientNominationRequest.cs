namespace Acme.Workforce.Talent.Nomination.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient nomination request is in its life cycle.</summary>
public enum LenientNominationRequestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient nomination request as the nomination team keeps it.</summary>
public sealed class LenientNominationRequest(string code, string label, string unit, double budget, double cost, double minimum, string region, LenientNominationRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public double Budget { get; set; } = budget;
    public double Cost { get; set; } = cost;
    public double Minimum { get; set; } = minimum;
    public string Region { get; set; } = region;
    public LenientNominationRequestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient nomination request in status created.</summary>
    public static LenientNominationRequest NewLenientNominationRequest(string code, string label)
    {
        return new LenientNominationRequest(
            code,
            label,
            "",
            0.0,
            0.0,
            0.0,
            "",
            LenientNominationRequestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientNominationRequestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientNominationRequestStatus next)
    {
        switch (Status)
        {
            case LenientNominationRequestStatus.Created:
                switch (next)
                {
                    case LenientNominationRequestStatus.Published:
                        return true;
                }
                return false;
            case LenientNominationRequestStatus.Validated:
                switch (next)
                {
                    case LenientNominationRequestStatus.Published:
                        return true;
                }
                return false;
            case LenientNominationRequestStatus.Published:
                return next == LenientNominationRequestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LenientNominationRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient nomination request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetLoad()
    {
        return Math.Round(Minimum + Budget, 2);
    }

    public LenientNominationRequest Copy()
    {
        return new LenientNominationRequest(Code, Label, Unit, Budget, Cost, Minimum, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
