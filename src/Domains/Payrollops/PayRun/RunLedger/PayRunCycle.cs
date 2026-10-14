namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run cycle is in its life cycle.</summary>
public enum PayRunCycleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pay run cycle and its current status.</summary>
public sealed class PayRunCycle(string code, string name, int retries, string title, int durationMinutes, Dictionary<string, string> attributes, int quantity, int capacity, int createdAt, string segment, PayRunCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Quantity { get; set; } = quantity;
    public int Capacity { get; set; } = capacity;
    public int CreatedAt { get; set; } = createdAt;
    public string Segment { get; set; } = segment;
    public PayRunCycleStatus Status { get; set; } = status;

    /// <summary>Starts a pay run cycle in status idle.</summary>
    public static PayRunCycle NewPayRunCycle(string code, string name)
    {
        return new PayRunCycle(
            code,
            name,
            0,
            "",
            0,
            new Dictionary<string, string>(),
            0,
            0,
            0,
            "",
            PayRunCycleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunCycleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunCycleStatus next)
    {
        switch (Status)
        {
            case PayRunCycleStatus.Idle:
                return next == PayRunCycleStatus.Warming;
            case PayRunCycleStatus.Warming:
                return next == PayRunCycleStatus.Draining;
            case PayRunCycleStatus.Ready:
                switch (next)
                {
                    case PayRunCycleStatus.Stopped:
                        return true;
                }
                return false;
            case PayRunCycleStatus.Draining:
                return next == PayRunCycleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PayRunCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double AdjustedLoad()
    {
        return Math.Round((double)CreatedAt * (double)DurationMinutes, 2);
    }

    public PayRunCycle Copy()
    {
        return new PayRunCycle(
            Code,
            Name,
            Retries,
            Title,
            DurationMinutes,
            Attributes,
            Quantity,
            Capacity,
            CreatedAt,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
