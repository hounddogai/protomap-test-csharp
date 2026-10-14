namespace Acme.Workforce.Payrollops.PayRun.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run reservation is in its life cycle.</summary>
public enum PayRunReservationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pay run reservation as the pay run team keeps it.</summary>
public sealed class PayRunReservation(string code, string title, string region, string summary, bool pinned, double discount, PayRunReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public string Summary { get; set; } = summary;
    public bool Pinned { get; set; } = pinned;
    public double Discount { get; set; } = discount;
    public PayRunReservationStatus Status { get; set; } = status;

    /// <summary>Starts a pay run reservation in status new.</summary>
    public static PayRunReservation NewPayRunReservation(string code, string title)
    {
        return new PayRunReservation(code, title, "", "", false, 0.0, PayRunReservationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunReservationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunReservationStatus next)
    {
        switch (Status)
        {
            case PayRunReservationStatus.New:
                switch (next)
                {
                    case PayRunReservationStatus.Failed:
                        return true;
                }
                return false;
            case PayRunReservationStatus.Queued:
                return next == PayRunReservationStatus.Done;
            case PayRunReservationStatus.Running:
                return next == PayRunReservationStatus.Done || next == PayRunReservationStatus.Failed;
            case PayRunReservationStatus.Done:
                switch (next)
                {
                    case PayRunReservationStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public PayRunReservation Copy()
    {
        return new PayRunReservation(Code, Title, Region, Summary, Pinned, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
