namespace Acme.Workforce.Talent.Mobility.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility segment is in its life cycle.</summary>
public enum MobilitySegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One mobility segment, with what the planners need to act on it.</summary>
public sealed class MobilitySegment(string code, string name, double amount, double minimum, int retries, int windowDays, string title, int limit, MobilitySegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public double Minimum { get; set; } = minimum;
    public int Retries { get; set; } = retries;
    public int WindowDays { get; set; } = windowDays;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public MobilitySegmentStatus Status { get; set; } = status;

    /// <summary>Starts a mobility segment in status requested.</summary>
    public static MobilitySegment NewMobilitySegment(string code, string name)
    {
        return new MobilitySegment(code, name, 0.0, 0.0, 0, 0, "", 0, MobilitySegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilitySegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilitySegmentStatus next)
    {
        switch (Status)
        {
            case MobilitySegmentStatus.Requested:
                switch (next)
                {
                    case MobilitySegmentStatus.Granted:
                        return true;
                }
                return false;
            case MobilitySegmentStatus.Granted:
                switch (next)
                {
                    case MobilitySegmentStatus.Revoked:
                    case MobilitySegmentStatus.Expired:
                        return true;
                }
                return false;
            case MobilitySegmentStatus.Revoked:
                switch (next)
                {
                    case MobilitySegmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilitySegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Amount - (double)Retries, 2);
    }

    public MobilitySegment Copy()
    {
        return new MobilitySegment(Code, Name, Amount, Minimum, Retries, WindowDays, Title, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
