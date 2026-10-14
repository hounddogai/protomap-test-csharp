namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span threshold is in its life cycle.</summary>
public enum SpanThresholdStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A span threshold. Its code is unique within a tenant.</summary>
public sealed class SpanThreshold(string code, string name, string checksum, string grade, string phone, string title, double amount, SpanThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public string Grade { get; set; } = grade;
    public string Phone { get; set; } = phone;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public SpanThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a span threshold in status proposed.</summary>
    public static SpanThreshold NewSpanThreshold(string code, string name)
    {
        return new SpanThreshold(code, name, "", "", "", "", 0.0, SpanThresholdStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanThresholdStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanThresholdStatus next)
    {
        switch (Status)
        {
            case SpanThresholdStatus.Proposed:
                switch (next)
                {
                    case SpanThresholdStatus.Rejected:
                        return true;
                }
                return false;
            case SpanThresholdStatus.Approved:
                return next == SpanThresholdStatus.Withdrawn;
            case SpanThresholdStatus.Rejected:
                switch (next)
                {
                    case SpanThresholdStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public SpanThreshold Copy()
    {
        return new SpanThreshold(Code, Name, Checksum, Grade, Phone, Title, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
