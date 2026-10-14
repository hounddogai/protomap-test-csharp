namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span preference is in its life cycle.</summary>
public enum SpanPreferenceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A span preference as the span team keeps it.</summary>
public sealed class SpanPreference(string code, string title, string name, string checksum, string timezone, bool archived, double score, SpanPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public string Timezone { get; set; } = timezone;
    public bool Archived { get; set; } = archived;
    public double Score { get; set; } = score;
    public SpanPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a span preference in status proposed.</summary>
    public static SpanPreference NewSpanPreference(string code, string title)
    {
        return new SpanPreference(code, title, "", "", "", false, 0.0, SpanPreferenceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanPreferenceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanPreferenceStatus next)
    {
        switch (Status)
        {
            case SpanPreferenceStatus.Proposed:
                return next == SpanPreferenceStatus.Approved;
            case SpanPreferenceStatus.Approved:
                switch (next)
                {
                    case SpanPreferenceStatus.Rejected:
                    case SpanPreferenceStatus.Withdrawn:
                        return true;
                }
                return false;
            case SpanPreferenceStatus.Rejected:
                switch (next)
                {
                    case SpanPreferenceStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public SpanPreference Copy()
    {
        return new SpanPreference(Code, Title, Name, Checksum, Timezone, Archived, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
