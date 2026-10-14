namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling recognition reservation is in its life cycle.</summary>
public enum RollingRecognitionReservationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rolling recognition reservation. Its code is unique within a tenant.</summary>
public sealed class RollingRecognitionReservation(string code, string title, string team, string segment, double cost, int updatedAt, RollingRecognitionReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string Segment { get; set; } = segment;
    public double Cost { get; set; } = cost;
    public int UpdatedAt { get; set; } = updatedAt;
    public RollingRecognitionReservationStatus Status { get; set; } = status;

    /// <summary>Starts a rolling recognition reservation in status open.</summary>
    public static RollingRecognitionReservation NewRollingRecognitionReservation(string code, string title)
    {
        return new RollingRecognitionReservation(
            code,
            title,
            "",
            "",
            0.0,
            0,
            RollingRecognitionReservationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRecognitionReservationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRecognitionReservationStatus next)
    {
        switch (Status)
        {
            case RollingRecognitionReservationStatus.Open:
                switch (next)
                {
                    case RollingRecognitionReservationStatus.InReview:
                        return true;
                }
                return false;
            case RollingRecognitionReservationStatus.InReview:
                return next == RollingRecognitionReservationStatus.Resolved || next == RollingRecognitionReservationStatus.Archived;
            case RollingRecognitionReservationStatus.Resolved:
                return next == RollingRecognitionReservationStatus.Reopened || next == RollingRecognitionReservationStatus.Archived;
            case RollingRecognitionReservationStatus.Reopened:
                switch (next)
                {
                    case RollingRecognitionReservationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingRecognitionReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling recognition reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"rolling recognition reservation"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)UpdatedAt + Cost, 2);
    }

    public RollingRecognitionReservation Copy()
    {
        return new RollingRecognitionReservation(Code, Title, Team, Segment, Cost, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
