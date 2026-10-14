namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance reservation is in its life cycle.</summary>
public enum BalanceReservationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A balance reservation as the balance team keeps it.</summary>
public sealed class BalanceReservation(string code, string title, string label, string source, double ratio, BalanceReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public double Ratio { get; set; } = ratio;
    public BalanceReservationStatus Status { get; set; } = status;

    /// <summary>Starts a balance reservation in status planned.</summary>
    public static BalanceReservation NewBalanceReservation(string code, string title)
    {
        return new BalanceReservation(code, title, "", "", 0.0, BalanceReservationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceReservationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceReservationStatus next)
    {
        switch (Status)
        {
            case BalanceReservationStatus.Planned:
                return next == BalanceReservationStatus.Scheduled;
            case BalanceReservationStatus.Scheduled:
                switch (next)
                {
                    case BalanceReservationStatus.InProgress:
                    case BalanceReservationStatus.Completed:
                        return true;
                }
                return false;
            case BalanceReservationStatus.InProgress:
                return next == BalanceReservationStatus.Completed;
            case BalanceReservationStatus.Completed:
                switch (next)
                {
                    case BalanceReservationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public BalanceReservation Copy()
    {
        return new BalanceReservation(Code, Title, Label, Source, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
