namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient span record is in its life cycle.</summary>
public enum LenientSpanRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One lenient span record, with what the planners need to act on it.</summary>
public sealed class LenientSpanRecord(string code, string label, string name, string skill, string timezone, string source, double amount, LenientSpanRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public string Timezone { get; set; } = timezone;
    public string Source { get; set; } = source;
    public double Amount { get; set; } = amount;
    public LenientSpanRecordStatus Status { get; set; } = status;

    /// <summary>Starts a lenient span record in status created.</summary>
    public static LenientSpanRecord NewLenientSpanRecord(string code, string label)
    {
        return new LenientSpanRecord(code, label, "", "", "", "", 0.0, LenientSpanRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientSpanRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientSpanRecordStatus next)
    {
        switch (Status)
        {
            case LenientSpanRecordStatus.Created:
                switch (next)
                {
                    case LenientSpanRecordStatus.Published:
                    case LenientSpanRecordStatus.Retired:
                        return true;
                }
                return false;
            case LenientSpanRecordStatus.Validated:
                switch (next)
                {
                    case LenientSpanRecordStatus.Published:
                        return true;
                }
                return false;
            case LenientSpanRecordStatus.Published:
                switch (next)
                {
                    case LenientSpanRecordStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientSpanRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient span record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public LenientSpanRecord Copy()
    {
        return new LenientSpanRecord(Code, Label, Name, Skill, Timezone, Source, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
