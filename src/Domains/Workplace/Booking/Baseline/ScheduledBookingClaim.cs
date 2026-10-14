namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled booking claim is in its life cycle.</summary>
public enum ScheduledBookingClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a scheduled booking claim and its current status.</summary>
public sealed class ScheduledBookingClaim(string code, string title, double rate, int attempts, bool pinned, int retries, string category, ScheduledBookingClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public int Attempts { get; set; } = attempts;
    public bool Pinned { get; set; } = pinned;
    public int Retries { get; set; } = retries;
    public string Category { get; set; } = category;
    public ScheduledBookingClaimStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled booking claim in status new.</summary>
    public static ScheduledBookingClaim NewScheduledBookingClaim(string code, string title)
    {
        return new ScheduledBookingClaim(code, title, 0.0, 0, false, 0, "", ScheduledBookingClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledBookingClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledBookingClaimStatus next)
    {
        switch (Status)
        {
            case ScheduledBookingClaimStatus.New:
                return next == ScheduledBookingClaimStatus.Queued || next == ScheduledBookingClaimStatus.Failed;
            case ScheduledBookingClaimStatus.Queued:
                return next == ScheduledBookingClaimStatus.Running || next == ScheduledBookingClaimStatus.Done;
            case ScheduledBookingClaimStatus.Running:
                switch (next)
                {
                    case ScheduledBookingClaimStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledBookingClaimStatus.Done:
                switch (next)
                {
                    case ScheduledBookingClaimStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledBookingClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled booking claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetLoad()
    {
        return Math.Round(Rate + (double)Retries, 2);
    }

    public ScheduledBookingClaim Copy()
    {
        return new ScheduledBookingClaim(Code, Title, Rate, Attempts, Pinned, Retries, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
