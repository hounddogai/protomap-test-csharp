namespace Acme.Workforce.Compensation.MeritCycle.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle reservation is in its life cycle.</summary>
public enum MeritCycleReservationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a merit cycle reservation and its current status.</summary>
public sealed class MeritCycleReservation(string code, string label, string locale, string source, string category, int retries, double budget, MeritCycleReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public string Source { get; set; } = source;
    public string Category { get; set; } = category;
    public int Retries { get; set; } = retries;
    public double Budget { get; set; } = budget;
    public MeritCycleReservationStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle reservation in status planned.</summary>
    public static MeritCycleReservation NewMeritCycleReservation(string code, string label)
    {
        return new MeritCycleReservation(code, label, "", "", "", 0, 0.0, MeritCycleReservationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleReservationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleReservationStatus next)
    {
        switch (Status)
        {
            case MeritCycleReservationStatus.Planned:
                return next == MeritCycleReservationStatus.InProgress || next == MeritCycleReservationStatus.Cancelled;
            case MeritCycleReservationStatus.Scheduled:
                return next == MeritCycleReservationStatus.Cancelled;
            case MeritCycleReservationStatus.InProgress:
                switch (next)
                {
                    case MeritCycleReservationStatus.Completed:
                        return true;
                }
                return false;
            case MeritCycleReservationStatus.Completed:
                return next == MeritCycleReservationStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(MeritCycleReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Budget + (double)Retries, 2);
    }

    public MeritCycleReservation Copy()
    {
        return new MeritCycleReservation(Code, Label, Locale, Source, Category, Retries, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
