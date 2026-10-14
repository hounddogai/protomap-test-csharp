namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg assignment is in its life cycle.</summary>
public enum ReorgAssignmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One reorg assignment, with what the planners need to act on it.</summary>
public sealed class ReorgAssignment(string code, string title, int graceDays, int batchSize, double margin, string reviewer, int retries, double cost, ReorgAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public int BatchSize { get; set; } = batchSize;
    public double Margin { get; set; } = margin;
    public string Reviewer { get; set; } = reviewer;
    public int Retries { get; set; } = retries;
    public double Cost { get; set; } = cost;
    public ReorgAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a reorg assignment in status requested.</summary>
    public static ReorgAssignment NewReorgAssignment(string code, string title)
    {
        return new ReorgAssignment(code, title, 0, 0, 0.0, "", 0, 0.0, ReorgAssignmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgAssignmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgAssignmentStatus next)
    {
        switch (Status)
        {
            case ReorgAssignmentStatus.Requested:
                return next == ReorgAssignmentStatus.Revoked;
            case ReorgAssignmentStatus.Granted:
                return next == ReorgAssignmentStatus.Revoked || next == ReorgAssignmentStatus.Expired;
            case ReorgAssignmentStatus.Revoked:
                return next == ReorgAssignmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ReorgAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetExposure()
    {
        return Math.Round((double)BatchSize + Margin, 2);
    }

    public ReorgAssignment Copy()
    {
        return new ReorgAssignment(Code, Title, GraceDays, BatchSize, Margin, Reviewer, Retries, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
