namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount request is in its life cycle.</summary>
public enum HeadcountRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A headcount request as the headcount team keeps it.</summary>
public sealed class HeadcountRequest(string code, string title, double cost, int durationMinutes, List<string> tags, string ownerId, bool archived, string reviewer, string employeeId, int sequence, HeadcountRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public int DurationMinutes { get; set; } = durationMinutes;
    public List<string> Tags { get; set; } = tags;
    public string OwnerId { get; set; } = ownerId;
    public bool Archived { get; set; } = archived;
    public string Reviewer { get; set; } = reviewer;
    public string EmployeeId { get; set; } = employeeId;
    public int Sequence { get; set; } = sequence;
    public HeadcountRequestStatus Status { get; set; } = status;

    /// <summary>Starts a headcount request in status planned.</summary>
    public static HeadcountRequest NewHeadcountRequest(string code, string title)
    {
        return new HeadcountRequest(
            code,
            title,
            0.0,
            0,
            new List<string>(),
            "",
            false,
            "",
            "",
            0,
            HeadcountRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountRequestStatus next)
    {
        switch (Status)
        {
            case HeadcountRequestStatus.Planned:
                switch (next)
                {
                    case HeadcountRequestStatus.InProgress:
                    case HeadcountRequestStatus.Cancelled:
                        return true;
                }
                return false;
            case HeadcountRequestStatus.Scheduled:
                switch (next)
                {
                    case HeadcountRequestStatus.Cancelled:
                        return true;
                }
                return false;
            case HeadcountRequestStatus.InProgress:
                return next == HeadcountRequestStatus.Cancelled;
            case HeadcountRequestStatus.Completed:
                switch (next)
                {
                    case HeadcountRequestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"headcount request"} {Code}";
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

    public double EffectiveValue()
    {
        return Math.Round(Cost + (double)DurationMinutes, 2);
    }

    public HeadcountRequest Copy()
    {
        return new HeadcountRequest(
            Code,
            Title,
            Cost,
            DurationMinutes,
            Tags,
            OwnerId,
            Archived,
            Reviewer,
            EmployeeId,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
