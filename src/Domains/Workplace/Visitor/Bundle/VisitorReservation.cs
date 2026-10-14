namespace Acme.Workforce.Workplace.Visitor.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor reservation is in its life cycle.</summary>
public enum VisitorReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A visitor reservation as the visitor team keeps it.</summary>
public sealed class VisitorReservation(string code, string title, int capacity, int sequence, string salaryBand, string timezone, int priority, string costCenter, double threshold, double budget, VisitorReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public int Sequence { get; set; } = sequence;
    public string SalaryBand { get; set; } = salaryBand;
    public string Timezone { get; set; } = timezone;
    public int Priority { get; set; } = priority;
    public string CostCenter { get; set; } = costCenter;
    public double Threshold { get; set; } = threshold;
    public double Budget { get; set; } = budget;
    public VisitorReservationStatus Status { get; set; } = status;

    /// <summary>Starts a visitor reservation in status requested.</summary>
    public static VisitorReservation NewVisitorReservation(string code, string title)
    {
        return new VisitorReservation(
            code,
            title,
            0,
            0,
            "",
            "",
            0,
            "",
            0.0,
            0.0,
            VisitorReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorReservationStatus next)
    {
        switch (Status)
        {
            case VisitorReservationStatus.Requested:
                return next == VisitorReservationStatus.Revoked || next == VisitorReservationStatus.Expired;
            case VisitorReservationStatus.Granted:
                return next == VisitorReservationStatus.Revoked || next == VisitorReservationStatus.Expired;
            case VisitorReservationStatus.Revoked:
                return next == VisitorReservationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(VisitorReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Threshold + (double)Capacity, 2);
    }

    public VisitorReservation Copy()
    {
        return new VisitorReservation(
            Code,
            Title,
            Capacity,
            Sequence,
            SalaryBand,
            Timezone,
            Priority,
            CostCenter,
            Threshold,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
