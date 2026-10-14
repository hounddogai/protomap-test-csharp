namespace Acme.Workforce.Compliance.PolicyAck.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary policy ack exception is in its life cycle.</summary>
public enum PrimaryPolicyAckExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A primary policy ack exception as the policy ack team keeps it.</summary>
public sealed class PrimaryPolicyAckException(string code, string title, double cost, int windowDays, int capacity, string jobCode, int limit, List<string> tags, PrimaryPolicyAckExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public int WindowDays { get; set; } = windowDays;
    public int Capacity { get; set; } = capacity;
    public string JobCode { get; set; } = jobCode;
    public int Limit { get; set; } = limit;
    public List<string> Tags { get; set; } = tags;
    public PrimaryPolicyAckExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a primary policy ack exception in status planned.</summary>
    public static PrimaryPolicyAckException NewPrimaryPolicyAckException(string code, string title)
    {
        return new PrimaryPolicyAckException(
            code,
            title,
            0.0,
            0,
            0,
            "",
            0,
            new List<string>(),
            PrimaryPolicyAckExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPolicyAckExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPolicyAckExceptionStatus next)
    {
        switch (Status)
        {
            case PrimaryPolicyAckExceptionStatus.Planned:
                switch (next)
                {
                    case PrimaryPolicyAckExceptionStatus.InProgress:
                        return true;
                }
                return false;
            case PrimaryPolicyAckExceptionStatus.Scheduled:
                return next == PrimaryPolicyAckExceptionStatus.Completed || next == PrimaryPolicyAckExceptionStatus.Cancelled;
            case PrimaryPolicyAckExceptionStatus.InProgress:
                switch (next)
                {
                    case PrimaryPolicyAckExceptionStatus.Completed:
                    case PrimaryPolicyAckExceptionStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryPolicyAckExceptionStatus.Completed:
                return next == PrimaryPolicyAckExceptionStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PrimaryPolicyAckExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary policy ack exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary policy ack exception"} {Code}";
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

    public double WeightedLoad()
    {
        return Math.Round((double)Capacity * Cost, 2);
    }

    public PrimaryPolicyAckException Copy()
    {
        return new PrimaryPolicyAckException(Code, Title, Cost, WindowDays, Capacity, JobCode, Limit, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
