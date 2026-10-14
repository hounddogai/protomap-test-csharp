namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band policy is in its life cycle.</summary>
public enum PayBandPolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One pay band policy, with what the planners need to act on it.</summary>
public sealed class PayBandPolicy(string code, string title, int capacity, bool approved, double rate, List<string> tags, PayBandPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public bool Approved { get; set; } = approved;
    public double Rate { get; set; } = rate;
    public List<string> Tags { get; set; } = tags;
    public PayBandPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a pay band policy in status planned.</summary>
    public static PayBandPolicy NewPayBandPolicy(string code, string title)
    {
        return new PayBandPolicy(code, title, 0, false, 0.0, new List<string>(), PayBandPolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandPolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandPolicyStatus next)
    {
        switch (Status)
        {
            case PayBandPolicyStatus.Planned:
                return next == PayBandPolicyStatus.Scheduled || next == PayBandPolicyStatus.Cancelled;
            case PayBandPolicyStatus.Scheduled:
                return next == PayBandPolicyStatus.InProgress || next == PayBandPolicyStatus.Cancelled;
            case PayBandPolicyStatus.InProgress:
                return next == PayBandPolicyStatus.Completed || next == PayBandPolicyStatus.Cancelled;
            case PayBandPolicyStatus.Completed:
                return next == PayBandPolicyStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PayBandPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pay band policy"} {Code}";
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
        return Math.Round((double)Capacity - Rate, 2);
    }

    public PayBandPolicy Copy()
    {
        return new PayBandPolicy(Code, Title, Capacity, Approved, Rate, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
