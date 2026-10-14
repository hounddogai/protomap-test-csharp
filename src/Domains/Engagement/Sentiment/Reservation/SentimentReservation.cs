namespace Acme.Workforce.Engagement.Sentiment.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment reservation is in its life cycle.</summary>
public enum SentimentReservationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A sentiment reservation. Its code is unique within a tenant.</summary>
public sealed class SentimentReservation(string code, string name, double discount, double weight, string reviewer, double threshold, int version, double minimum, string team, SentimentReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public double Weight { get; set; } = weight;
    public string Reviewer { get; set; } = reviewer;
    public double Threshold { get; set; } = threshold;
    public int Version { get; set; } = version;
    public double Minimum { get; set; } = minimum;
    public string Team { get; set; } = team;
    public SentimentReservationStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment reservation in status draft.</summary>
    public static SentimentReservation NewSentimentReservation(string code, string name)
    {
        return new SentimentReservation(code, name, 0.0, 0.0, "", 0.0, 0, 0.0, "", SentimentReservationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentReservationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentReservationStatus next)
    {
        switch (Status)
        {
            case SentimentReservationStatus.Draft:
                switch (next)
                {
                    case SentimentReservationStatus.Suspended:
                        return true;
                }
                return false;
            case SentimentReservationStatus.Pending:
                return next == SentimentReservationStatus.Active || next == SentimentReservationStatus.Suspended;
            case SentimentReservationStatus.Active:
                switch (next)
                {
                    case SentimentReservationStatus.Suspended:
                    case SentimentReservationStatus.Closed:
                        return true;
                }
                return false;
            case SentimentReservationStatus.Suspended:
                switch (next)
                {
                    case SentimentReservationStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Threshold * Minimum, 2);
    }

    public SentimentReservation Copy()
    {
        return new SentimentReservation(
            Code,
            Name,
            Discount,
            Weight,
            Reviewer,
            Threshold,
            Version,
            Minimum,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
