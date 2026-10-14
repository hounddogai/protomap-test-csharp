namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span entry is in its life cycle.</summary>
public enum SpanEntryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A span entry as the span team keeps it.</summary>
public sealed class SpanEntry(string code, string name, string team, string skill, string currency, double score, SpanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public string Skill { get; set; } = skill;
    public string Currency { get; set; } = currency;
    public double Score { get; set; } = score;
    public SpanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a span entry in status draft.</summary>
    public static SpanEntry NewSpanEntry(string code, string name)
    {
        return new SpanEntry(code, name, "", "", "", 0.0, SpanEntryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanEntryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanEntryStatus next)
    {
        switch (Status)
        {
            case SpanEntryStatus.Draft:
                switch (next)
                {
                    case SpanEntryStatus.Closed:
                        return true;
                }
                return false;
            case SpanEntryStatus.Pending:
                switch (next)
                {
                    case SpanEntryStatus.Closed:
                        return true;
                }
                return false;
            case SpanEntryStatus.Active:
                switch (next)
                {
                    case SpanEntryStatus.Suspended:
                    case SpanEntryStatus.Closed:
                        return true;
                }
                return false;
            case SpanEntryStatus.Suspended:
                switch (next)
                {
                    case SpanEntryStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"span entry"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public SpanEntry Copy()
    {
        return new SpanEntry(Code, Name, Team, Skill, Currency, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
