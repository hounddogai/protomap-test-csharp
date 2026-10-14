namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment group is in its life cycle.</summary>
public enum AssignmentGroupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A assignment group. Its code is unique within a tenant.</summary>
public sealed class AssignmentGroup(string code, string label, string description, Dictionary<string, string> attributes, string locale, int durationMinutes, AssignmentGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Locale { get; set; } = locale;
    public int DurationMinutes { get; set; } = durationMinutes;
    public AssignmentGroupStatus Status { get; set; } = status;

    /// <summary>Starts a assignment group in status new.</summary>
    public static AssignmentGroup NewAssignmentGroup(string code, string label)
    {
        return new AssignmentGroup(
            code,
            label,
            "",
            new Dictionary<string, string>(),
            "",
            0,
            AssignmentGroupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentGroupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentGroupStatus next)
    {
        switch (Status)
        {
            case AssignmentGroupStatus.New:
                return next == AssignmentGroupStatus.Done || next == AssignmentGroupStatus.Failed;
            case AssignmentGroupStatus.Queued:
                return next == AssignmentGroupStatus.Done || next == AssignmentGroupStatus.Failed;
            case AssignmentGroupStatus.Running:
                return next == AssignmentGroupStatus.Done || next == AssignmentGroupStatus.Failed;
            case AssignmentGroupStatus.Done:
                return next == AssignmentGroupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AssignmentGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment group {Code} cannot move from {Status} to {next}");
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

    public AssignmentGroup Copy()
    {
        return new AssignmentGroup(Code, Label, Description, Attributes, Locale, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
