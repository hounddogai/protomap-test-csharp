namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary visitor assignment is in its life cycle.</summary>
public enum PrimaryVisitorAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A primary visitor assignment as the visitor team keeps it.</summary>
public sealed class PrimaryVisitorAssignment(string code, string label, int windowDays, string name, int capacity, Dictionary<string, string> attributes, PrimaryVisitorAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public PrimaryVisitorAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a primary visitor assignment in status open.</summary>
    public static PrimaryVisitorAssignment NewPrimaryVisitorAssignment(string code, string label)
    {
        return new PrimaryVisitorAssignment(
            code,
            label,
            0,
            "",
            0,
            new Dictionary<string, string>(),
            PrimaryVisitorAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryVisitorAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryVisitorAssignmentStatus next)
    {
        switch (Status)
        {
            case PrimaryVisitorAssignmentStatus.Open:
                return next == PrimaryVisitorAssignmentStatus.InReview || next == PrimaryVisitorAssignmentStatus.Resolved;
            case PrimaryVisitorAssignmentStatus.InReview:
                return next == PrimaryVisitorAssignmentStatus.Resolved || next == PrimaryVisitorAssignmentStatus.Archived;
            case PrimaryVisitorAssignmentStatus.Resolved:
                switch (next)
                {
                    case PrimaryVisitorAssignmentStatus.Reopened:
                    case PrimaryVisitorAssignmentStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryVisitorAssignmentStatus.Reopened:
                return next == PrimaryVisitorAssignmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PrimaryVisitorAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary visitor assignment {Code} cannot move from {Status} to {next}");
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

    public double AdjustedLoad()
    {
        return Math.Round((double)Capacity - (double)WindowDays, 2);
    }

    public PrimaryVisitorAssignment Copy()
    {
        return new PrimaryVisitorAssignment(Code, Label, WindowDays, Name, Capacity, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
