namespace Acme.Workforce.Payrollops.PayRun.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run target is in its life cycle.</summary>
public enum PayRunTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One pay run target, with what the planners need to act on it.</summary>
public sealed class PayRunTarget(string code, string label, Dictionary<string, string> attributes, int expiresAt, string department, string phone, int durationMinutes, int sequence, string title, PayRunTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Department { get; set; } = department;
    public string Phone { get; set; } = phone;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Sequence { get; set; } = sequence;
    public string Title { get; set; } = title;
    public PayRunTargetStatus Status { get; set; } = status;

    /// <summary>Starts a pay run target in status planned.</summary>
    public static PayRunTarget NewPayRunTarget(string code, string label)
    {
        return new PayRunTarget(
            code,
            label,
            new Dictionary<string, string>(),
            0,
            "",
            "",
            0,
            0,
            "",
            PayRunTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunTargetStatus next)
    {
        switch (Status)
        {
            case PayRunTargetStatus.Planned:
                switch (next)
                {
                    case PayRunTargetStatus.Scheduled:
                    case PayRunTargetStatus.Completed:
                        return true;
                }
                return false;
            case PayRunTargetStatus.Scheduled:
                switch (next)
                {
                    case PayRunTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case PayRunTargetStatus.InProgress:
                return next == PayRunTargetStatus.Completed || next == PayRunTargetStatus.Cancelled;
            case PayRunTargetStatus.Completed:
                switch (next)
                {
                    case PayRunTargetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetLoad()
    {
        return Math.Round((double)ExpiresAt * (double)DurationMinutes, 2);
    }

    public PayRunTarget Copy()
    {
        return new PayRunTarget(
            Code,
            Label,
            Attributes,
            ExpiresAt,
            Department,
            Phone,
            DurationMinutes,
            Sequence,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
