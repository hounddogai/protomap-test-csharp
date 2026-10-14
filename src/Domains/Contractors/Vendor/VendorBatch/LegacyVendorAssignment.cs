namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy vendor assignment is in its life cycle.</summary>
public enum LegacyVendorAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One legacy vendor assignment, with what the planners need to act on it.</summary>
public sealed class LegacyVendorAssignment(string code, string label, string channel, string badge, int attempts, Dictionary<string, string> attributes, int updatedAt, string location, string category, double minimum, int priority, LegacyVendorAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string Badge { get; set; } = badge;
    public int Attempts { get; set; } = attempts;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Location { get; set; } = location;
    public string Category { get; set; } = category;
    public double Minimum { get; set; } = minimum;
    public int Priority { get; set; } = priority;
    public LegacyVendorAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a legacy vendor assignment in status idle.</summary>
    public static LegacyVendorAssignment NewLegacyVendorAssignment(string code, string label)
    {
        return new LegacyVendorAssignment(
            code,
            label,
            "",
            "",
            0,
            new Dictionary<string, string>(),
            0,
            "",
            "",
            0.0,
            0,
            LegacyVendorAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyVendorAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyVendorAssignmentStatus next)
    {
        switch (Status)
        {
            case LegacyVendorAssignmentStatus.Idle:
                return next == LegacyVendorAssignmentStatus.Ready;
            case LegacyVendorAssignmentStatus.Warming:
                switch (next)
                {
                    case LegacyVendorAssignmentStatus.Draining:
                    case LegacyVendorAssignmentStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyVendorAssignmentStatus.Ready:
                switch (next)
                {
                    case LegacyVendorAssignmentStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyVendorAssignmentStatus.Draining:
                switch (next)
                {
                    case LegacyVendorAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyVendorAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy vendor assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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
        return Math.Round((double)UpdatedAt + (double)Priority, 2);
    }

    public LegacyVendorAssignment Copy()
    {
        return new LegacyVendorAssignment(
            Code,
            Label,
            Channel,
            Badge,
            Attempts,
            Attributes,
            UpdatedAt,
            Location,
            Category,
            Minimum,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
