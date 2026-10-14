namespace Acme.Workforce.Absence.Policy.PolicyRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict policy assignment is in its life cycle.</summary>
public enum StrictPolicyAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a strict policy assignment and its current status.</summary>
public sealed class StrictPolicyAssignment(string code, string label, int capacity, string segment, Dictionary<string, string> attributes, string externalRef, StrictPolicyAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public string Segment { get; set; } = segment;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ExternalRef { get; set; } = externalRef;
    public StrictPolicyAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a strict policy assignment in status draft.</summary>
    public static StrictPolicyAssignment NewStrictPolicyAssignment(string code, string label)
    {
        return new StrictPolicyAssignment(
            code,
            label,
            0,
            "",
            new Dictionary<string, string>(),
            "",
            StrictPolicyAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPolicyAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPolicyAssignmentStatus next)
    {
        switch (Status)
        {
            case StrictPolicyAssignmentStatus.Draft:
                switch (next)
                {
                    case StrictPolicyAssignmentStatus.Pending:
                    case StrictPolicyAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case StrictPolicyAssignmentStatus.Pending:
                switch (next)
                {
                    case StrictPolicyAssignmentStatus.Active:
                    case StrictPolicyAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case StrictPolicyAssignmentStatus.Active:
                switch (next)
                {
                    case StrictPolicyAssignmentStatus.Suspended:
                        return true;
                }
                return false;
            case StrictPolicyAssignmentStatus.Suspended:
                return next == StrictPolicyAssignmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(StrictPolicyAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict policy assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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
        return (double)Capacity;
    }

    public StrictPolicyAssignment Copy()
    {
        return new StrictPolicyAssignment(Code, Label, Capacity, Segment, Attributes, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
