namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance digest is in its life cycle.</summary>
public enum BalanceDigestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A balance digest as the balance team keeps it.</summary>
public sealed class BalanceDigest(string code, string title, int capacity, int attempts, string segment, double floorValue, BalanceDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public int Attempts { get; set; } = attempts;
    public string Segment { get; set; } = segment;
    public double FloorValue { get; set; } = floorValue;
    public BalanceDigestStatus Status { get; set; } = status;

    /// <summary>Starts a balance digest in status planned.</summary>
    public static BalanceDigest NewBalanceDigest(string code, string title)
    {
        return new BalanceDigest(code, title, 0, 0, "", 0.0, BalanceDigestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceDigestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceDigestStatus next)
    {
        switch (Status)
        {
            case BalanceDigestStatus.Planned:
                switch (next)
                {
                    case BalanceDigestStatus.InProgress:
                    case BalanceDigestStatus.Cancelled:
                        return true;
                }
                return false;
            case BalanceDigestStatus.Scheduled:
                switch (next)
                {
                    case BalanceDigestStatus.Completed:
                    case BalanceDigestStatus.Cancelled:
                        return true;
                }
                return false;
            case BalanceDigestStatus.InProgress:
                return next == BalanceDigestStatus.Cancelled;
            case BalanceDigestStatus.Completed:
                return next == BalanceDigestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BalanceDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(FloorValue * (double)Capacity, 2);
    }

    public BalanceDigest Copy()
    {
        return new BalanceDigest(Code, Title, Capacity, Attempts, Segment, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
