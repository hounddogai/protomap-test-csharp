namespace Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding entry is in its life cycle.</summary>
public enum OffboardingEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A offboarding entry as the offboarding team keeps it.</summary>
public sealed class OffboardingEntry(string code, string name, string segment, Dictionary<string, string> attributes, string unit, string title, double score, OffboardingEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Unit { get; set; } = unit;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public OffboardingEntryStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding entry in status new.</summary>
    public static OffboardingEntry NewOffboardingEntry(string code, string name)
    {
        return new OffboardingEntry(
            code,
            name,
            "",
            new Dictionary<string, string>(),
            "",
            "",
            0.0,
            OffboardingEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingEntryStatus next)
    {
        switch (Status)
        {
            case OffboardingEntryStatus.New:
                return next == OffboardingEntryStatus.Running;
            case OffboardingEntryStatus.Queued:
                return next == OffboardingEntryStatus.Failed;
            case OffboardingEntryStatus.Running:
                return next == OffboardingEntryStatus.Done;
            case OffboardingEntryStatus.Done:
                return next == OffboardingEntryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(OffboardingEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"offboarding entry"} {Code}";
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
        return Score;
    }

    public OffboardingEntry Copy()
    {
        return new OffboardingEntry(Code, Name, Segment, Attributes, Unit, Title, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
