namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement request is in its life cycle.</summary>
public enum EngagementRequestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a engagement request and its current status.</summary>
public sealed class EngagementRequest(string code, string label, int leaveDays, double floorValue, double hours, int capacity, Dictionary<string, string> attributes, string contactEmail, double discount, double rate, int updatedAt, EngagementRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public double FloorValue { get; set; } = floorValue;
    public double Hours { get; set; } = hours;
    public int Capacity { get; set; } = capacity;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ContactEmail { get; set; } = contactEmail;
    public double Discount { get; set; } = discount;
    public double Rate { get; set; } = rate;
    public int UpdatedAt { get; set; } = updatedAt;
    public EngagementRequestStatus Status { get; set; } = status;

    /// <summary>Starts a engagement request in status idle.</summary>
    public static EngagementRequest NewEngagementRequest(string code, string label)
    {
        return new EngagementRequest(
            code,
            label,
            0,
            0.0,
            0.0,
            0,
            new Dictionary<string, string>(),
            "",
            0.0,
            0.0,
            0,
            EngagementRequestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementRequestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementRequestStatus next)
    {
        switch (Status)
        {
            case EngagementRequestStatus.Idle:
                return next == EngagementRequestStatus.Draining || next == EngagementRequestStatus.Stopped;
            case EngagementRequestStatus.Warming:
                return next == EngagementRequestStatus.Ready;
            case EngagementRequestStatus.Ready:
                switch (next)
                {
                    case EngagementRequestStatus.Draining:
                        return true;
                }
                return false;
            case EngagementRequestStatus.Draining:
                return next == EngagementRequestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(EngagementRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"engagement request"} {Code}";
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

    public double WeightedLoad()
    {
        return Math.Round((double)LeaveDays * Hours, 2);
    }

    public EngagementRequest Copy()
    {
        return new EngagementRequest(
            Code,
            Label,
            LeaveDays,
            FloorValue,
            Hours,
            Capacity,
            Attributes,
            ContactEmail,
            Discount,
            Rate,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
