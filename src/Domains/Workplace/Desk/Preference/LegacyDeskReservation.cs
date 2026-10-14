namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy desk reservation is in its life cycle.</summary>
public enum LegacyDeskReservationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a legacy desk reservation and its current status.</summary>
public sealed class LegacyDeskReservation(string code, string label, bool pinned, int batchSize, string locale, string reviewer, LegacyDeskReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public int BatchSize { get; set; } = batchSize;
    public string Locale { get; set; } = locale;
    public string Reviewer { get; set; } = reviewer;
    public LegacyDeskReservationStatus Status { get; set; } = status;

    /// <summary>Starts a legacy desk reservation in status new.</summary>
    public static LegacyDeskReservation NewLegacyDeskReservation(string code, string label)
    {
        return new LegacyDeskReservation(code, label, false, 0, "", "", LegacyDeskReservationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyDeskReservationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyDeskReservationStatus next)
    {
        switch (Status)
        {
            case LegacyDeskReservationStatus.New:
                return next == LegacyDeskReservationStatus.Queued || next == LegacyDeskReservationStatus.Done;
            case LegacyDeskReservationStatus.Queued:
                return next == LegacyDeskReservationStatus.Running || next == LegacyDeskReservationStatus.Done;
            case LegacyDeskReservationStatus.Running:
                return next == LegacyDeskReservationStatus.Failed;
            case LegacyDeskReservationStatus.Done:
                return next == LegacyDeskReservationStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LegacyDeskReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy desk reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public LegacyDeskReservation Copy()
    {
        return new LegacyDeskReservation(Code, Label, Pinned, BatchSize, Locale, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
