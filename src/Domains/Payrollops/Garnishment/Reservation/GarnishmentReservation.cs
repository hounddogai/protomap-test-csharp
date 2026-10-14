namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment reservation is in its life cycle.</summary>
public enum GarnishmentReservationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a garnishment reservation and its current status.</summary>
public sealed class GarnishmentReservation(string code, string label, string description, List<string> tags, string reviewer, int limit, int createdAt, int durationMinutes, bool archived, bool visible, string salaryBand, GarnishmentReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public List<string> Tags { get; set; } = tags;
    public string Reviewer { get; set; } = reviewer;
    public int Limit { get; set; } = limit;
    public int CreatedAt { get; set; } = createdAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Archived { get; set; } = archived;
    public bool Visible { get; set; } = visible;
    public string SalaryBand { get; set; } = salaryBand;
    public GarnishmentReservationStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment reservation in status planned.</summary>
    public static GarnishmentReservation NewGarnishmentReservation(string code, string label)
    {
        return new GarnishmentReservation(
            code,
            label,
            "",
            new List<string>(),
            "",
            0,
            0,
            0,
            false,
            false,
            "",
            GarnishmentReservationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentReservationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentReservationStatus next)
    {
        switch (Status)
        {
            case GarnishmentReservationStatus.Planned:
                switch (next)
                {
                    case GarnishmentReservationStatus.Completed:
                    case GarnishmentReservationStatus.Cancelled:
                        return true;
                }
                return false;
            case GarnishmentReservationStatus.Scheduled:
                switch (next)
                {
                    case GarnishmentReservationStatus.Cancelled:
                        return true;
                }
                return false;
            case GarnishmentReservationStatus.InProgress:
                return next == GarnishmentReservationStatus.Completed;
            case GarnishmentReservationStatus.Completed:
                return next == GarnishmentReservationStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(GarnishmentReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"garnishment reservation"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Limit + (double)DurationMinutes, 2);
    }

    public GarnishmentReservation Copy()
    {
        return new GarnishmentReservation(
            Code,
            Label,
            Description,
            Tags,
            Reviewer,
            Limit,
            CreatedAt,
            DurationMinutes,
            Archived,
            Visible,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
