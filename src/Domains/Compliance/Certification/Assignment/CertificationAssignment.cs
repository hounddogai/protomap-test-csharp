namespace Acme.Workforce.Compliance.Certification.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification assignment is in its life cycle.</summary>
public enum CertificationAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One certification assignment, with what the planners need to act on it.</summary>
public sealed class CertificationAssignment(string code, string label, string department, int durationMinutes, double hours, Dictionary<string, string> attributes, CertificationAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Hours { get; set; } = hours;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public CertificationAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a certification assignment in status idle.</summary>
    public static CertificationAssignment NewCertificationAssignment(string code, string label)
    {
        return new CertificationAssignment(
            code,
            label,
            "",
            0,
            0.0,
            new Dictionary<string, string>(),
            CertificationAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationAssignmentStatus next)
    {
        switch (Status)
        {
            case CertificationAssignmentStatus.Idle:
                return next == CertificationAssignmentStatus.Ready || next == CertificationAssignmentStatus.Stopped;
            case CertificationAssignmentStatus.Warming:
                return next == CertificationAssignmentStatus.Ready || next == CertificationAssignmentStatus.Draining;
            case CertificationAssignmentStatus.Ready:
                return next == CertificationAssignmentStatus.Draining;
            case CertificationAssignmentStatus.Draining:
                switch (next)
                {
                    case CertificationAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
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

    public double NetVolume()
    {
        return Math.Round((double)DurationMinutes - Hours, 2);
    }

    public CertificationAssignment Copy()
    {
        return new CertificationAssignment(Code, Label, Department, DurationMinutes, Hours, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
