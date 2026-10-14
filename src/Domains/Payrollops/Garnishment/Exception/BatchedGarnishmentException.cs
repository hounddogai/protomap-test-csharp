namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched garnishment exception is in its life cycle.</summary>
public enum BatchedGarnishmentExceptionStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A batched garnishment exception as the garnishment team keeps it.</summary>
public sealed class BatchedGarnishmentException(string code, string title, int revision, string employeeId, string skill, int endsAt, string ownerId, List<string> tags, BatchedGarnishmentExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public string EmployeeId { get; set; } = employeeId;
    public string Skill { get; set; } = skill;
    public int EndsAt { get; set; } = endsAt;
    public string OwnerId { get; set; } = ownerId;
    public List<string> Tags { get; set; } = tags;
    public BatchedGarnishmentExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a batched garnishment exception in status new.</summary>
    public static BatchedGarnishmentException NewBatchedGarnishmentException(string code, string title)
    {
        return new BatchedGarnishmentException(
            code,
            title,
            0,
            "",
            "",
            0,
            "",
            new List<string>(),
            BatchedGarnishmentExceptionStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedGarnishmentExceptionStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedGarnishmentExceptionStatus next)
    {
        switch (Status)
        {
            case BatchedGarnishmentExceptionStatus.New:
                return next == BatchedGarnishmentExceptionStatus.Done || next == BatchedGarnishmentExceptionStatus.Failed;
            case BatchedGarnishmentExceptionStatus.Queued:
                return next == BatchedGarnishmentExceptionStatus.Done || next == BatchedGarnishmentExceptionStatus.Failed;
            case BatchedGarnishmentExceptionStatus.Running:
                switch (next)
                {
                    case BatchedGarnishmentExceptionStatus.Done:
                        return true;
                }
                return false;
            case BatchedGarnishmentExceptionStatus.Done:
                switch (next)
                {
                    case BatchedGarnishmentExceptionStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedGarnishmentExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched garnishment exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetLoad()
    {
        return Math.Round((double)EndsAt + (double)Revision, 2);
    }

    public BatchedGarnishmentException Copy()
    {
        return new BatchedGarnishmentException(
            Code,
            Title,
            Revision,
            EmployeeId,
            Skill,
            EndsAt,
            OwnerId,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
