namespace Acme.Workforce.Assets.Refresh.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh reservation is in its life cycle.</summary>
public enum RefreshReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A refresh reservation. Its code is unique within a tenant.</summary>
public sealed class RefreshReservation(string code, string name, bool pinned, string channel, double fte, RefreshReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public string Channel { get; set; } = channel;
    public double Fte { get; set; } = fte;
    public RefreshReservationStatus Status { get; set; } = status;

    /// <summary>Starts a refresh reservation in status idle.</summary>
    public static RefreshReservation NewRefreshReservation(string code, string name)
    {
        return new RefreshReservation(code, name, false, "", 0.0, RefreshReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshReservationStatus next)
    {
        switch (Status)
        {
            case RefreshReservationStatus.Idle:
                switch (next)
                {
                    case RefreshReservationStatus.Ready:
                        return true;
                }
                return false;
            case RefreshReservationStatus.Warming:
                switch (next)
                {
                    case RefreshReservationStatus.Draining:
                        return true;
                }
                return false;
            case RefreshReservationStatus.Ready:
                return next == RefreshReservationStatus.Stopped;
            case RefreshReservationStatus.Draining:
                switch (next)
                {
                    case RefreshReservationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public RefreshReservation Copy()
    {
        return new RefreshReservation(Code, Name, Pinned, Channel, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
