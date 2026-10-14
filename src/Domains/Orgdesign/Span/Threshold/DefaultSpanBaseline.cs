namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default span baseline is in its life cycle.</summary>
public enum DefaultSpanBaselineStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a default span baseline and its current status.</summary>
public sealed class DefaultSpanBaseline(string code, string name, string label, bool archived, string summary, string jobCode, string phone, double amount, DefaultSpanBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string Summary { get; set; } = summary;
    public string JobCode { get; set; } = jobCode;
    public string Phone { get; set; } = phone;
    public double Amount { get; set; } = amount;
    public DefaultSpanBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a default span baseline in status idle.</summary>
    public static DefaultSpanBaseline NewDefaultSpanBaseline(string code, string name)
    {
        return new DefaultSpanBaseline(code, name, "", false, "", "", "", 0.0, DefaultSpanBaselineStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultSpanBaselineStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultSpanBaselineStatus next)
    {
        switch (Status)
        {
            case DefaultSpanBaselineStatus.Idle:
                return next == DefaultSpanBaselineStatus.Draining;
            case DefaultSpanBaselineStatus.Warming:
                return next == DefaultSpanBaselineStatus.Stopped;
            case DefaultSpanBaselineStatus.Ready:
                return next == DefaultSpanBaselineStatus.Draining;
            case DefaultSpanBaselineStatus.Draining:
                switch (next)
                {
                    case DefaultSpanBaselineStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultSpanBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default span baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public DefaultSpanBaseline Copy()
    {
        return new DefaultSpanBaseline(Code, Name, Label, Archived, Summary, JobCode, Phone, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
