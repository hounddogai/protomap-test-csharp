namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient visitor preference is in its life cycle.</summary>
public enum LenientVisitorPreferenceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One lenient visitor preference, with what the planners need to act on it.</summary>
public sealed class LenientVisitorPreference(string code, string label, Dictionary<string, string> attributes, double floorValue, string category, LenientVisitorPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double FloorValue { get; set; } = floorValue;
    public string Category { get; set; } = category;
    public LenientVisitorPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a lenient visitor preference in status draft.</summary>
    public static LenientVisitorPreference NewLenientVisitorPreference(string code, string label)
    {
        return new LenientVisitorPreference(
            code,
            label,
            new Dictionary<string, string>(),
            0.0,
            "",
            LenientVisitorPreferenceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientVisitorPreferenceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientVisitorPreferenceStatus next)
    {
        switch (Status)
        {
            case LenientVisitorPreferenceStatus.Draft:
                return next == LenientVisitorPreferenceStatus.Active || next == LenientVisitorPreferenceStatus.Closed;
            case LenientVisitorPreferenceStatus.Pending:
                return next == LenientVisitorPreferenceStatus.Active;
            case LenientVisitorPreferenceStatus.Active:
                return next == LenientVisitorPreferenceStatus.Suspended || next == LenientVisitorPreferenceStatus.Closed;
            case LenientVisitorPreferenceStatus.Suspended:
                switch (next)
                {
                    case LenientVisitorPreferenceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientVisitorPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient visitor preference {Code} cannot move from {Status} to {next}");
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

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public LenientVisitorPreference Copy()
    {
        return new LenientVisitorPreference(Code, Label, Attributes, FloorValue, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
