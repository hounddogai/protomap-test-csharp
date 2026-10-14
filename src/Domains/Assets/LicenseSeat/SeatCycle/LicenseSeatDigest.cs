namespace Acme.Workforce.Assets.LicenseSeat.SeatCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat digest is in its life cycle.</summary>
public enum LicenseSeatDigestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One license seat digest, with what the planners need to act on it.</summary>
public sealed class LicenseSeatDigest(string code, string name, int sequence, double weight, double cost, LicenseSeatDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public double Weight { get; set; } = weight;
    public double Cost { get; set; } = cost;
    public LicenseSeatDigestStatus Status { get; set; } = status;

    /// <summary>Starts a license seat digest in status planned.</summary>
    public static LicenseSeatDigest NewLicenseSeatDigest(string code, string name)
    {
        return new LicenseSeatDigest(code, name, 0, 0.0, 0.0, LicenseSeatDigestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatDigestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatDigestStatus next)
    {
        switch (Status)
        {
            case LicenseSeatDigestStatus.Planned:
                switch (next)
                {
                    case LicenseSeatDigestStatus.Scheduled:
                    case LicenseSeatDigestStatus.InProgress:
                        return true;
                }
                return false;
            case LicenseSeatDigestStatus.Scheduled:
                return next == LicenseSeatDigestStatus.InProgress;
            case LicenseSeatDigestStatus.InProgress:
                return next == LicenseSeatDigestStatus.Completed;
            case LicenseSeatDigestStatus.Completed:
                switch (next)
                {
                    case LicenseSeatDigestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"license seat digest"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Sequence - Cost, 2);
    }

    public LicenseSeatDigest Copy()
    {
        return new LicenseSeatDigest(Code, Name, Sequence, Weight, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
