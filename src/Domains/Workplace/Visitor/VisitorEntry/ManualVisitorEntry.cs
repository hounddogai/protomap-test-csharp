namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual visitor entry is in its life cycle.</summary>
public enum ManualVisitorEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A manual visitor entry. Its code is unique within a tenant.</summary>
public sealed class ManualVisitorEntry(string code, string label, Dictionary<string, string> attributes, double minimum, string segment, string region, ManualVisitorEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Minimum { get; set; } = minimum;
    public string Segment { get; set; } = segment;
    public string Region { get; set; } = region;
    public ManualVisitorEntryStatus Status { get; set; } = status;

    /// <summary>Starts a manual visitor entry in status proposed.</summary>
    public static ManualVisitorEntry NewManualVisitorEntry(string code, string label)
    {
        return new ManualVisitorEntry(
            code,
            label,
            new Dictionary<string, string>(),
            0.0,
            "",
            "",
            ManualVisitorEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualVisitorEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualVisitorEntryStatus next)
    {
        switch (Status)
        {
            case ManualVisitorEntryStatus.Proposed:
                return next == ManualVisitorEntryStatus.Rejected;
            case ManualVisitorEntryStatus.Approved:
                switch (next)
                {
                    case ManualVisitorEntryStatus.Withdrawn:
                        return true;
                }
                return false;
            case ManualVisitorEntryStatus.Rejected:
                switch (next)
                {
                    case ManualVisitorEntryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualVisitorEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual visitor entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"manual visitor entry"} {Code}";
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
        return Minimum;
    }

    public ManualVisitorEntry Copy()
    {
        return new ManualVisitorEntry(Code, Label, Attributes, Minimum, Segment, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
