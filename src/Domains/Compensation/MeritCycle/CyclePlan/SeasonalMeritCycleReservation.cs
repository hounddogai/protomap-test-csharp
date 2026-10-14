namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal merit cycle reservation is in its life cycle.</summary>
public enum SeasonalMeritCycleReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A seasonal merit cycle reservation as the merit cycle team keeps it.</summary>
public sealed class SeasonalMeritCycleReservation(string code, string title, double threshold, string notes, double ceiling, int leaveDays, string grade, double margin, Dictionary<string, string> attributes, string summary, int quantity, SeasonalMeritCycleReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string Notes { get; set; } = notes;
    public double Ceiling { get; set; } = ceiling;
    public int LeaveDays { get; set; } = leaveDays;
    public string Grade { get; set; } = grade;
    public double Margin { get; set; } = margin;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Summary { get; set; } = summary;
    public int Quantity { get; set; } = quantity;
    public SeasonalMeritCycleReservationStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal merit cycle reservation in status requested.</summary>
    public static SeasonalMeritCycleReservation NewSeasonalMeritCycleReservation(string code, string title)
    {
        return new SeasonalMeritCycleReservation(
            code,
            title,
            0.0,
            "",
            0.0,
            0,
            "",
            0.0,
            new Dictionary<string, string>(),
            "",
            0,
            SeasonalMeritCycleReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalMeritCycleReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalMeritCycleReservationStatus next)
    {
        switch (Status)
        {
            case SeasonalMeritCycleReservationStatus.Requested:
                return next == SeasonalMeritCycleReservationStatus.Granted || next == SeasonalMeritCycleReservationStatus.Revoked;
            case SeasonalMeritCycleReservationStatus.Granted:
                return next == SeasonalMeritCycleReservationStatus.Revoked || next == SeasonalMeritCycleReservationStatus.Expired;
            case SeasonalMeritCycleReservationStatus.Revoked:
                switch (next)
                {
                    case SeasonalMeritCycleReservationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalMeritCycleReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal merit cycle reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetValue()
    {
        return Math.Round(Ceiling + (double)Quantity, 2);
    }

    public SeasonalMeritCycleReservation Copy()
    {
        return new SeasonalMeritCycleReservation(
            Code,
            Title,
            Threshold,
            Notes,
            Ceiling,
            LeaveDays,
            Grade,
            Margin,
            Attributes,
            Summary,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
