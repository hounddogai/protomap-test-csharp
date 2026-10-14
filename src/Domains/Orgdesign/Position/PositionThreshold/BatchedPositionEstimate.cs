namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched position estimate is in its life cycle.</summary>
public enum BatchedPositionEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A batched position estimate. Its code is unique within a tenant.</summary>
public sealed class BatchedPositionEstimate(string code, string name, string location, int priority, Dictionary<string, string> attributes, int attempts, int quantity, int expiresAt, string summary, double minimum, BatchedPositionEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public int Priority { get; set; } = priority;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Attempts { get; set; } = attempts;
    public int Quantity { get; set; } = quantity;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Summary { get; set; } = summary;
    public double Minimum { get; set; } = minimum;
    public BatchedPositionEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a batched position estimate in status idle.</summary>
    public static BatchedPositionEstimate NewBatchedPositionEstimate(string code, string name)
    {
        return new BatchedPositionEstimate(
            code,
            name,
            "",
            0,
            new Dictionary<string, string>(),
            0,
            0,
            0,
            "",
            0.0,
            BatchedPositionEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPositionEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPositionEstimateStatus next)
    {
        switch (Status)
        {
            case BatchedPositionEstimateStatus.Idle:
                switch (next)
                {
                    case BatchedPositionEstimateStatus.Ready:
                        return true;
                }
                return false;
            case BatchedPositionEstimateStatus.Warming:
                return next == BatchedPositionEstimateStatus.Draining || next == BatchedPositionEstimateStatus.Stopped;
            case BatchedPositionEstimateStatus.Ready:
                return next == BatchedPositionEstimateStatus.Draining || next == BatchedPositionEstimateStatus.Stopped;
            case BatchedPositionEstimateStatus.Draining:
                return next == BatchedPositionEstimateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BatchedPositionEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched position estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double EffectiveValue()
    {
        return Math.Round((double)Quantity - (double)Attempts, 2);
    }

    public BatchedPositionEstimate Copy()
    {
        return new BatchedPositionEstimate(
            Code,
            Name,
            Location,
            Priority,
            Attributes,
            Attempts,
            Quantity,
            ExpiresAt,
            Summary,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
