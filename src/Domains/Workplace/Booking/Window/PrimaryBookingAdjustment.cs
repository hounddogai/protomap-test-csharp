namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary booking adjustment is in its life cycle.</summary>
public enum PrimaryBookingAdjustmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One primary booking adjustment, with what the planners need to act on it.</summary>
public sealed class PrimaryBookingAdjustment(string code, string name, string department, int endsAt, int retries, double weight, string description, string externalRef, string costCenter, PrimaryBookingAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public int EndsAt { get; set; } = endsAt;
    public int Retries { get; set; } = retries;
    public double Weight { get; set; } = weight;
    public string Description { get; set; } = description;
    public string ExternalRef { get; set; } = externalRef;
    public string CostCenter { get; set; } = costCenter;
    public PrimaryBookingAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a primary booking adjustment in status open.</summary>
    public static PrimaryBookingAdjustment NewPrimaryBookingAdjustment(string code, string name)
    {
        return new PrimaryBookingAdjustment(
            code,
            name,
            "",
            0,
            0,
            0.0,
            "",
            "",
            "",
            PrimaryBookingAdjustmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryBookingAdjustmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryBookingAdjustmentStatus next)
    {
        switch (Status)
        {
            case PrimaryBookingAdjustmentStatus.Open:
                switch (next)
                {
                    case PrimaryBookingAdjustmentStatus.InReview:
                    case PrimaryBookingAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryBookingAdjustmentStatus.InReview:
                return next == PrimaryBookingAdjustmentStatus.Archived;
            case PrimaryBookingAdjustmentStatus.Resolved:
                switch (next)
                {
                    case PrimaryBookingAdjustmentStatus.Reopened:
                    case PrimaryBookingAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryBookingAdjustmentStatus.Reopened:
                switch (next)
                {
                    case PrimaryBookingAdjustmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryBookingAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary booking adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetValue()
    {
        return Math.Round(Weight + (double)Retries, 2);
    }

    public PrimaryBookingAdjustment Copy()
    {
        return new PrimaryBookingAdjustment(
            Code,
            Name,
            Department,
            EndsAt,
            Retries,
            Weight,
            Description,
            ExternalRef,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
