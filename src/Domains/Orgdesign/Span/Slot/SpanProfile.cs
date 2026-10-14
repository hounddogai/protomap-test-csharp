namespace Acme.Workforce.Orgdesign.Span.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span profile is in its life cycle.</summary>
public enum SpanProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One span profile, with what the planners need to act on it.</summary>
public sealed class SpanProfile(string code, string label, string costCenter, string source, double amount, string salaryBand, SpanProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public string Source { get; set; } = source;
    public double Amount { get; set; } = amount;
    public string SalaryBand { get; set; } = salaryBand;
    public SpanProfileStatus Status { get; set; } = status;

    /// <summary>Starts a span profile in status idle.</summary>
    public static SpanProfile NewSpanProfile(string code, string label)
    {
        return new SpanProfile(code, label, "", "", 0.0, "", SpanProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanProfileStatus next)
    {
        switch (Status)
        {
            case SpanProfileStatus.Idle:
                return next == SpanProfileStatus.Warming || next == SpanProfileStatus.Stopped;
            case SpanProfileStatus.Warming:
                switch (next)
                {
                    case SpanProfileStatus.Draining:
                    case SpanProfileStatus.Stopped:
                        return true;
                }
                return false;
            case SpanProfileStatus.Ready:
                return next == SpanProfileStatus.Draining;
            case SpanProfileStatus.Draining:
                switch (next)
                {
                    case SpanProfileStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span profile {Code} cannot move from {Status} to {next}");
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

    public SpanProfile Copy()
    {
        return new SpanProfile(Code, Label, CostCenter, Source, Amount, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
