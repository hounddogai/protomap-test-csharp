namespace Acme.Workforce.Orgdesign.Scenario.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped scenario reservation is in its life cycle.</summary>
public enum CappedScenarioReservationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a capped scenario reservation and its current status.</summary>
public sealed class CappedScenarioReservation(string code, string label, string summary, int retries, int priority, double rate, int tenureMonths, double cost, CappedScenarioReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public int Retries { get; set; } = retries;
    public int Priority { get; set; } = priority;
    public double Rate { get; set; } = rate;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Cost { get; set; } = cost;
    public CappedScenarioReservationStatus Status { get; set; } = status;

    /// <summary>Starts a capped scenario reservation in status created.</summary>
    public static CappedScenarioReservation NewCappedScenarioReservation(string code, string label)
    {
        return new CappedScenarioReservation(
            code,
            label,
            "",
            0,
            0,
            0.0,
            0,
            0.0,
            CappedScenarioReservationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedScenarioReservationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedScenarioReservationStatus next)
    {
        switch (Status)
        {
            case CappedScenarioReservationStatus.Created:
                switch (next)
                {
                    case CappedScenarioReservationStatus.Published:
                        return true;
                }
                return false;
            case CappedScenarioReservationStatus.Validated:
                switch (next)
                {
                    case CappedScenarioReservationStatus.Published:
                    case CappedScenarioReservationStatus.Retired:
                        return true;
                }
                return false;
            case CappedScenarioReservationStatus.Published:
                switch (next)
                {
                    case CappedScenarioReservationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedScenarioReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped scenario reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Priority * (double)Retries, 2);
    }

    public CappedScenarioReservation Copy()
    {
        return new CappedScenarioReservation(
            Code,
            Label,
            Summary,
            Retries,
            Priority,
            Rate,
            TenureMonths,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
