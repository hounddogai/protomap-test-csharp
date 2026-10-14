namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive merit cycle reservation is in its life cycle.</summary>
public enum AdaptiveMeritCycleReservationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A adaptive merit cycle reservation. Its code is unique within a tenant.</summary>
public sealed class AdaptiveMeritCycleReservation(string code, string title, int endsAt, string timezone, string skill, string category, AdaptiveMeritCycleReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public string Timezone { get; set; } = timezone;
    public string Skill { get; set; } = skill;
    public string Category { get; set; } = category;
    public AdaptiveMeritCycleReservationStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive merit cycle reservation in status draft.</summary>
    public static AdaptiveMeritCycleReservation NewAdaptiveMeritCycleReservation(string code, string title)
    {
        return new AdaptiveMeritCycleReservation(
            code,
            title,
            0,
            "",
            "",
            "",
            AdaptiveMeritCycleReservationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveMeritCycleReservationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveMeritCycleReservationStatus next)
    {
        switch (Status)
        {
            case AdaptiveMeritCycleReservationStatus.Draft:
                switch (next)
                {
                    case AdaptiveMeritCycleReservationStatus.Pending:
                    case AdaptiveMeritCycleReservationStatus.Active:
                        return true;
                }
                return false;
            case AdaptiveMeritCycleReservationStatus.Pending:
                return next == AdaptiveMeritCycleReservationStatus.Suspended || next == AdaptiveMeritCycleReservationStatus.Closed;
            case AdaptiveMeritCycleReservationStatus.Active:
                return next == AdaptiveMeritCycleReservationStatus.Closed;
            case AdaptiveMeritCycleReservationStatus.Suspended:
                return next == AdaptiveMeritCycleReservationStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AdaptiveMeritCycleReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive merit cycle reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"adaptive merit cycle reservation"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public AdaptiveMeritCycleReservation Copy()
    {
        return new AdaptiveMeritCycleReservation(Code, Title, EndsAt, Timezone, Skill, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
