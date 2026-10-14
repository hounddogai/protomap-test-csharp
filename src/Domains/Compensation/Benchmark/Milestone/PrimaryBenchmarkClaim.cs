namespace Acme.Workforce.Compensation.Benchmark.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary benchmark claim is in its life cycle.</summary>
public enum PrimaryBenchmarkClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A primary benchmark claim as the benchmark team keeps it.</summary>
public sealed class PrimaryBenchmarkClaim(string code, string title, string location, string ownerId, int leaveDays, PrimaryBenchmarkClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public string OwnerId { get; set; } = ownerId;
    public int LeaveDays { get; set; } = leaveDays;
    public PrimaryBenchmarkClaimStatus Status { get; set; } = status;

    /// <summary>Starts a primary benchmark claim in status new.</summary>
    public static PrimaryBenchmarkClaim NewPrimaryBenchmarkClaim(string code, string title)
    {
        return new PrimaryBenchmarkClaim(code, title, "", "", 0, PrimaryBenchmarkClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryBenchmarkClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryBenchmarkClaimStatus next)
    {
        switch (Status)
        {
            case PrimaryBenchmarkClaimStatus.New:
                switch (next)
                {
                    case PrimaryBenchmarkClaimStatus.Done:
                    case PrimaryBenchmarkClaimStatus.Failed:
                        return true;
                }
                return false;
            case PrimaryBenchmarkClaimStatus.Queued:
                return next == PrimaryBenchmarkClaimStatus.Running;
            case PrimaryBenchmarkClaimStatus.Running:
                switch (next)
                {
                    case PrimaryBenchmarkClaimStatus.Done:
                        return true;
                }
                return false;
            case PrimaryBenchmarkClaimStatus.Done:
                return next == PrimaryBenchmarkClaimStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PrimaryBenchmarkClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary benchmark claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary benchmark claim"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public PrimaryBenchmarkClaim Copy()
    {
        return new PrimaryBenchmarkClaim(Code, Title, Location, OwnerId, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
