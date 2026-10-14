namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay segment is in its life cycle.</summary>
public enum RetroPaySegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a retro pay segment and its current status.</summary>
public sealed class RetroPaySegment(string code, string title, double ceiling, string region, string location, string contactEmail, string name, string team, RetroPaySegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public string Region { get; set; } = region;
    public string Location { get; set; } = location;
    public string ContactEmail { get; set; } = contactEmail;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public RetroPaySegmentStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay segment in status idle.</summary>
    public static RetroPaySegment NewRetroPaySegment(string code, string title)
    {
        return new RetroPaySegment(code, title, 0.0, "", "", "", "", "", RetroPaySegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPaySegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPaySegmentStatus next)
    {
        switch (Status)
        {
            case RetroPaySegmentStatus.Idle:
                return next == RetroPaySegmentStatus.Warming || next == RetroPaySegmentStatus.Draining;
            case RetroPaySegmentStatus.Warming:
                switch (next)
                {
                    case RetroPaySegmentStatus.Ready:
                    case RetroPaySegmentStatus.Stopped:
                        return true;
                }
                return false;
            case RetroPaySegmentStatus.Ready:
                switch (next)
                {
                    case RetroPaySegmentStatus.Draining:
                    case RetroPaySegmentStatus.Stopped:
                        return true;
                }
                return false;
            case RetroPaySegmentStatus.Draining:
                return next == RetroPaySegmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RetroPaySegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public RetroPaySegment Copy()
    {
        return new RetroPaySegment(Code, Title, Ceiling, Region, Location, ContactEmail, Name, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
