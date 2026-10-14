namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding assignment is in its life cycle.</summary>
public enum OffboardingAssignmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A offboarding assignment. Its code is unique within a tenant.</summary>
public sealed class OffboardingAssignment(string code, string label, Dictionary<string, string> attributes, int durationMinutes, string badge, bool pinned, OffboardingAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Badge { get; set; } = badge;
    public bool Pinned { get; set; } = pinned;
    public OffboardingAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding assignment in status new.</summary>
    public static OffboardingAssignment NewOffboardingAssignment(string code, string label)
    {
        return new OffboardingAssignment(
            code,
            label,
            new Dictionary<string, string>(),
            0,
            "",
            false,
            OffboardingAssignmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingAssignmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingAssignmentStatus next)
    {
        switch (Status)
        {
            case OffboardingAssignmentStatus.New:
                switch (next)
                {
                    case OffboardingAssignmentStatus.Queued:
                    case OffboardingAssignmentStatus.Running:
                        return true;
                }
                return false;
            case OffboardingAssignmentStatus.Queued:
                return next == OffboardingAssignmentStatus.Failed;
            case OffboardingAssignmentStatus.Running:
                switch (next)
                {
                    case OffboardingAssignmentStatus.Done:
                    case OffboardingAssignmentStatus.Failed:
                        return true;
                }
                return false;
            case OffboardingAssignmentStatus.Done:
                switch (next)
                {
                    case OffboardingAssignmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding assignment {Code} cannot move from {Status} to {next}");
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

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public OffboardingAssignment Copy()
    {
        return new OffboardingAssignment(Code, Label, Attributes, DurationMinutes, Badge, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
