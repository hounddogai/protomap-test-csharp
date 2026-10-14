namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment reservation is in its life cycle.</summary>
public enum AdjustmentReservationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One adjustment reservation, with what the planners need to act on it.</summary>
public sealed class AdjustmentReservation(string code, string name, string contactEmail, int priority, string summary, double weight, string externalRef, string jobCode, int retries, int leaveDays, double margin, AdjustmentReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public int Priority { get; set; } = priority;
    public string Summary { get; set; } = summary;
    public double Weight { get; set; } = weight;
    public string ExternalRef { get; set; } = externalRef;
    public string JobCode { get; set; } = jobCode;
    public int Retries { get; set; } = retries;
    public int LeaveDays { get; set; } = leaveDays;
    public double Margin { get; set; } = margin;
    public AdjustmentReservationStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment reservation in status open.</summary>
    public static AdjustmentReservation NewAdjustmentReservation(string code, string name)
    {
        return new AdjustmentReservation(
            code,
            name,
            "",
            0,
            "",
            0.0,
            "",
            "",
            0,
            0,
            0.0,
            AdjustmentReservationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentReservationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentReservationStatus next)
    {
        switch (Status)
        {
            case AdjustmentReservationStatus.Open:
                return next == AdjustmentReservationStatus.Resolved;
            case AdjustmentReservationStatus.InReview:
                return next == AdjustmentReservationStatus.Reopened;
            case AdjustmentReservationStatus.Resolved:
                return next == AdjustmentReservationStatus.Archived;
            case AdjustmentReservationStatus.Reopened:
                switch (next)
                {
                    case AdjustmentReservationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Priority - Weight, 2);
    }

    public AdjustmentReservation Copy()
    {
        return new AdjustmentReservation(
            Code,
            Name,
            ContactEmail,
            Priority,
            Summary,
            Weight,
            ExternalRef,
            JobCode,
            Retries,
            LeaveDays,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
