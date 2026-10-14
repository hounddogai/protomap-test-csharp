namespace Acme.Workforce.Compensation.PayBand.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band reservation is in its life cycle.</summary>
public enum PayBandReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pay band reservation. Its code is unique within a tenant.</summary>
public sealed class PayBandReservation(string code, string name, string channel, double budget, string jobCode, int durationMinutes, int tenureMonths, string salaryBand, string timezone, int retries, string managerId, PayBandReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public string JobCode { get; set; } = jobCode;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int TenureMonths { get; set; } = tenureMonths;
    public string SalaryBand { get; set; } = salaryBand;
    public string Timezone { get; set; } = timezone;
    public int Retries { get; set; } = retries;
    public string ManagerId { get; set; } = managerId;
    public PayBandReservationStatus Status { get; set; } = status;

    /// <summary>Starts a pay band reservation in status proposed.</summary>
    public static PayBandReservation NewPayBandReservation(string code, string name)
    {
        return new PayBandReservation(
            code,
            name,
            "",
            0.0,
            "",
            0,
            0,
            "",
            "",
            0,
            "",
            PayBandReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandReservationStatus next)
    {
        switch (Status)
        {
            case PayBandReservationStatus.Proposed:
                return next == PayBandReservationStatus.Approved;
            case PayBandReservationStatus.Approved:
                return next == PayBandReservationStatus.Withdrawn;
            case PayBandReservationStatus.Rejected:
                return next == PayBandReservationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PayBandReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pay band reservation"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Retries - (double)DurationMinutes, 2);
    }

    public PayBandReservation Copy()
    {
        return new PayBandReservation(
            Code,
            Name,
            Channel,
            Budget,
            JobCode,
            DurationMinutes,
            TenureMonths,
            SalaryBand,
            Timezone,
            Retries,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
