namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey reservation is in its life cycle.</summary>
public enum PulseSurveyReservationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A pulse survey reservation. Its code is unique within a tenant.</summary>
public sealed class PulseSurveyReservation(string code, string title, double ceiling, int revision, string label, int batchSize, double fte, int quantity, double floorValue, PulseSurveyReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public int Revision { get; set; } = revision;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public double Fte { get; set; } = fte;
    public int Quantity { get; set; } = quantity;
    public double FloorValue { get; set; } = floorValue;
    public PulseSurveyReservationStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey reservation in status created.</summary>
    public static PulseSurveyReservation NewPulseSurveyReservation(string code, string title)
    {
        return new PulseSurveyReservation(
            code,
            title,
            0.0,
            0,
            "",
            0,
            0.0,
            0,
            0.0,
            PulseSurveyReservationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyReservationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyReservationStatus next)
    {
        switch (Status)
        {
            case PulseSurveyReservationStatus.Created:
                switch (next)
                {
                    case PulseSurveyReservationStatus.Published:
                    case PulseSurveyReservationStatus.Retired:
                        return true;
                }
                return false;
            case PulseSurveyReservationStatus.Validated:
                return next == PulseSurveyReservationStatus.Retired;
            case PulseSurveyReservationStatus.Published:
                switch (next)
                {
                    case PulseSurveyReservationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Fte + Ceiling, 2);
    }

    public PulseSurveyReservation Copy()
    {
        return new PulseSurveyReservation(
            Code,
            Title,
            Ceiling,
            Revision,
            Label,
            BatchSize,
            Fte,
            Quantity,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
