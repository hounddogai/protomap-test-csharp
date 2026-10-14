namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending career path batch is in its life cycle.</summary>
public enum PendingCareerPathBatchStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pending career path batch as the career path team keeps it.</summary>
public sealed class PendingCareerPathBatch(string code, string title, int createdAt, string employeeId, int batchSize, double hours, string contactEmail, PendingCareerPathBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public string EmployeeId { get; set; } = employeeId;
    public int BatchSize { get; set; } = batchSize;
    public double Hours { get; set; } = hours;
    public string ContactEmail { get; set; } = contactEmail;
    public PendingCareerPathBatchStatus Status { get; set; } = status;

    /// <summary>Starts a pending career path batch in status planned.</summary>
    public static PendingCareerPathBatch NewPendingCareerPathBatch(string code, string title)
    {
        return new PendingCareerPathBatch(code, title, 0, "", 0, 0.0, "", PendingCareerPathBatchStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingCareerPathBatchStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingCareerPathBatchStatus next)
    {
        switch (Status)
        {
            case PendingCareerPathBatchStatus.Planned:
                switch (next)
                {
                    case PendingCareerPathBatchStatus.InProgress:
                        return true;
                }
                return false;
            case PendingCareerPathBatchStatus.Scheduled:
                return next == PendingCareerPathBatchStatus.InProgress || next == PendingCareerPathBatchStatus.Cancelled;
            case PendingCareerPathBatchStatus.InProgress:
                return next == PendingCareerPathBatchStatus.Cancelled;
            case PendingCareerPathBatchStatus.Completed:
                switch (next)
                {
                    case PendingCareerPathBatchStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingCareerPathBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending career path batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pending career path batch"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)CreatedAt - (double)BatchSize, 2);
    }

    public PendingCareerPathBatch Copy()
    {
        return new PendingCareerPathBatch(
            Code,
            Title,
            CreatedAt,
            EmployeeId,
            BatchSize,
            Hours,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
