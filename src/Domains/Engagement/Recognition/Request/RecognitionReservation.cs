namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition reservation is in its life cycle.</summary>
public enum RecognitionReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A recognition reservation. Its code is unique within a tenant.</summary>
public sealed class RecognitionReservation(string code, string title, string ownerId, string timezone, int limit, string skill, string location, int graceDays, string category, string unit, RecognitionReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public string Timezone { get; set; } = timezone;
    public int Limit { get; set; } = limit;
    public string Skill { get; set; } = skill;
    public string Location { get; set; } = location;
    public int GraceDays { get; set; } = graceDays;
    public string Category { get; set; } = category;
    public string Unit { get; set; } = unit;
    public RecognitionReservationStatus Status { get; set; } = status;

    /// <summary>Starts a recognition reservation in status idle.</summary>
    public static RecognitionReservation NewRecognitionReservation(string code, string title)
    {
        return new RecognitionReservation(
            code,
            title,
            "",
            "",
            0,
            "",
            "",
            0,
            "",
            "",
            RecognitionReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionReservationStatus next)
    {
        switch (Status)
        {
            case RecognitionReservationStatus.Idle:
                return next == RecognitionReservationStatus.Draining;
            case RecognitionReservationStatus.Warming:
                switch (next)
                {
                    case RecognitionReservationStatus.Stopped:
                        return true;
                }
                return false;
            case RecognitionReservationStatus.Ready:
                return next == RecognitionReservationStatus.Draining || next == RecognitionReservationStatus.Stopped;
            case RecognitionReservationStatus.Draining:
                return next == RecognitionReservationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RecognitionReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)GraceDays - (double)Limit, 2);
    }

    public RecognitionReservation Copy()
    {
        return new RecognitionReservation(
            Code,
            Title,
            OwnerId,
            Timezone,
            Limit,
            Skill,
            Location,
            GraceDays,
            Category,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
