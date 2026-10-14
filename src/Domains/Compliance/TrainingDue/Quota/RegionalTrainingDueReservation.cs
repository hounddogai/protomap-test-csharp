namespace Acme.Workforce.Compliance.TrainingDue.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional training due reservation is in its life cycle.</summary>
public enum RegionalTrainingDueReservationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A regional training due reservation. Its code is unique within a tenant.</summary>
public sealed class RegionalTrainingDueReservation(string code, string name, string jobCode, double rating, double amount, int updatedAt, string category, string badge, double ratio, double minimum, string channel, RegionalTrainingDueReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public double Rating { get; set; } = rating;
    public double Amount { get; set; } = amount;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Category { get; set; } = category;
    public string Badge { get; set; } = badge;
    public double Ratio { get; set; } = ratio;
    public double Minimum { get; set; } = minimum;
    public string Channel { get; set; } = channel;
    public RegionalTrainingDueReservationStatus Status { get; set; } = status;

    /// <summary>Starts a regional training due reservation in status new.</summary>
    public static RegionalTrainingDueReservation NewRegionalTrainingDueReservation(string code, string name)
    {
        return new RegionalTrainingDueReservation(
            code,
            name,
            "",
            0.0,
            0.0,
            0,
            "",
            "",
            0.0,
            0.0,
            "",
            RegionalTrainingDueReservationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalTrainingDueReservationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalTrainingDueReservationStatus next)
    {
        switch (Status)
        {
            case RegionalTrainingDueReservationStatus.New:
                return next == RegionalTrainingDueReservationStatus.Running || next == RegionalTrainingDueReservationStatus.Failed;
            case RegionalTrainingDueReservationStatus.Queued:
                return next == RegionalTrainingDueReservationStatus.Failed;
            case RegionalTrainingDueReservationStatus.Running:
                return next == RegionalTrainingDueReservationStatus.Done || next == RegionalTrainingDueReservationStatus.Failed;
            case RegionalTrainingDueReservationStatus.Done:
                return next == RegionalTrainingDueReservationStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RegionalTrainingDueReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional training due reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Amount * Ratio, 2);
    }

    public RegionalTrainingDueReservation Copy()
    {
        return new RegionalTrainingDueReservation(
            Code,
            Name,
            JobCode,
            Rating,
            Amount,
            UpdatedAt,
            Category,
            Badge,
            Ratio,
            Minimum,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
