namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run segment is in its life cycle.</summary>
public enum PayRunSegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A pay run segment as the pay run team keeps it.</summary>
public sealed class PayRunSegment(string code, string label, int attempts, string ownerId, string region, double rating, string team, string locale, int revision, PayRunSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public string OwnerId { get; set; } = ownerId;
    public string Region { get; set; } = region;
    public double Rating { get; set; } = rating;
    public string Team { get; set; } = team;
    public string Locale { get; set; } = locale;
    public int Revision { get; set; } = revision;
    public PayRunSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a pay run segment in status idle.</summary>
    public static PayRunSegment NewPayRunSegment(string code, string label)
    {
        return new PayRunSegment(code, label, 0, "", "", 0.0, "", "", 0, PayRunSegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunSegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunSegmentStatus next)
    {
        switch (Status)
        {
            case PayRunSegmentStatus.Idle:
                switch (next)
                {
                    case PayRunSegmentStatus.Ready:
                        return true;
                }
                return false;
            case PayRunSegmentStatus.Warming:
                return next == PayRunSegmentStatus.Draining || next == PayRunSegmentStatus.Stopped;
            case PayRunSegmentStatus.Ready:
                switch (next)
                {
                    case PayRunSegmentStatus.Draining:
                    case PayRunSegmentStatus.Stopped:
                        return true;
                }
                return false;
            case PayRunSegmentStatus.Draining:
                return next == PayRunSegmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PayRunSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Rating - (double)Attempts, 2);
    }

    public PayRunSegment Copy()
    {
        return new PayRunSegment(Code, Label, Attempts, OwnerId, Region, Rating, Team, Locale, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
