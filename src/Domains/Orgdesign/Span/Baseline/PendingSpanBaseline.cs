namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending span baseline is in its life cycle.</summary>
public enum PendingSpanBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pending span baseline. Its code is unique within a tenant.</summary>
public sealed class PendingSpanBaseline(string code, string label, int priority, Dictionary<string, string> attributes, string jobCode, string country, string team, string reviewer, PendingSpanBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string JobCode { get; set; } = jobCode;
    public string Country { get; set; } = country;
    public string Team { get; set; } = team;
    public string Reviewer { get; set; } = reviewer;
    public PendingSpanBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a pending span baseline in status proposed.</summary>
    public static PendingSpanBaseline NewPendingSpanBaseline(string code, string label)
    {
        return new PendingSpanBaseline(
            code,
            label,
            0,
            new Dictionary<string, string>(),
            "",
            "",
            "",
            "",
            PendingSpanBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingSpanBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingSpanBaselineStatus next)
    {
        switch (Status)
        {
            case PendingSpanBaselineStatus.Proposed:
                return next == PendingSpanBaselineStatus.Approved;
            case PendingSpanBaselineStatus.Approved:
                switch (next)
                {
                    case PendingSpanBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
            case PendingSpanBaselineStatus.Rejected:
                switch (next)
                {
                    case PendingSpanBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingSpanBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending span baseline {Code} cannot move from {Status} to {next}");
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
        return (double)Priority;
    }

    public PendingSpanBaseline Copy()
    {
        return new PendingSpanBaseline(Code, Label, Priority, Attributes, JobCode, Country, Team, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
