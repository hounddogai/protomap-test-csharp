namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback leave request adjustment is in its life cycle.</summary>
public enum FallbackLeaveRequestAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A fallback leave request adjustment. Its code is unique within a tenant.</summary>
public sealed class FallbackLeaveRequestAdjustment(string code, string label, string phone, Dictionary<string, string> attributes, string country, int capacity, string managerId, double ratio, FallbackLeaveRequestAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Country { get; set; } = country;
    public int Capacity { get; set; } = capacity;
    public string ManagerId { get; set; } = managerId;
    public double Ratio { get; set; } = ratio;
    public FallbackLeaveRequestAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a fallback leave request adjustment in status idle.</summary>
    public static FallbackLeaveRequestAdjustment NewFallbackLeaveRequestAdjustment(string code, string label)
    {
        return new FallbackLeaveRequestAdjustment(
            code,
            label,
            "",
            new Dictionary<string, string>(),
            "",
            0,
            "",
            0.0,
            FallbackLeaveRequestAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackLeaveRequestAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackLeaveRequestAdjustmentStatus next)
    {
        switch (Status)
        {
            case FallbackLeaveRequestAdjustmentStatus.Idle:
                switch (next)
                {
                    case FallbackLeaveRequestAdjustmentStatus.Warming:
                    case FallbackLeaveRequestAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case FallbackLeaveRequestAdjustmentStatus.Warming:
                return next == FallbackLeaveRequestAdjustmentStatus.Draining;
            case FallbackLeaveRequestAdjustmentStatus.Ready:
                switch (next)
                {
                    case FallbackLeaveRequestAdjustmentStatus.Draining:
                        return true;
                }
                return false;
            case FallbackLeaveRequestAdjustmentStatus.Draining:
                return next == FallbackLeaveRequestAdjustmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FallbackLeaveRequestAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback leave request adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double EffectiveLoad()
    {
        return Math.Round((double)Capacity + Ratio, 2);
    }

    public FallbackLeaveRequestAdjustment Copy()
    {
        return new FallbackLeaveRequestAdjustment(
            Code,
            Label,
            Phone,
            Attributes,
            Country,
            Capacity,
            ManagerId,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
