namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling sentiment reservation is in its life cycle.</summary>
public enum RollingSentimentReservationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A rolling sentiment reservation. Its code is unique within a tenant.</summary>
public sealed class RollingSentimentReservation(string code, string name, string employeeId, double ceiling, string salaryBand, RollingSentimentReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public double Ceiling { get; set; } = ceiling;
    public string SalaryBand { get; set; } = salaryBand;
    public RollingSentimentReservationStatus Status { get; set; } = status;

    /// <summary>Starts a rolling sentiment reservation in status created.</summary>
    public static RollingSentimentReservation NewRollingSentimentReservation(string code, string name)
    {
        return new RollingSentimentReservation(code, name, "", 0.0, "", RollingSentimentReservationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingSentimentReservationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingSentimentReservationStatus next)
    {
        switch (Status)
        {
            case RollingSentimentReservationStatus.Created:
                return next == RollingSentimentReservationStatus.Published || next == RollingSentimentReservationStatus.Retired;
            case RollingSentimentReservationStatus.Validated:
                return next == RollingSentimentReservationStatus.Published;
            case RollingSentimentReservationStatus.Published:
                switch (next)
                {
                    case RollingSentimentReservationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingSentimentReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling sentiment reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public RollingSentimentReservation Copy()
    {
        return new RollingSentimentReservation(Code, Name, EmployeeId, Ceiling, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
