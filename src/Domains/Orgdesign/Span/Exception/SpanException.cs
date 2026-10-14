namespace Acme.Workforce.Orgdesign.Span.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span exception is in its life cycle.</summary>
public enum SpanExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A span exception. Its code is unique within a tenant.</summary>
public sealed class SpanException(string code, string label, string costCenter, int retries, string channel, SpanExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public int Retries { get; set; } = retries;
    public string Channel { get; set; } = channel;
    public SpanExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a span exception in status proposed.</summary>
    public static SpanException NewSpanException(string code, string label)
    {
        return new SpanException(code, label, "", 0, "", SpanExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanExceptionStatus next)
    {
        switch (Status)
        {
            case SpanExceptionStatus.Proposed:
                switch (next)
                {
                    case SpanExceptionStatus.Rejected:
                    case SpanExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
            case SpanExceptionStatus.Approved:
                return next == SpanExceptionStatus.Withdrawn;
            case SpanExceptionStatus.Rejected:
                return next == SpanExceptionStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SpanExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public SpanException Copy()
    {
        return new SpanException(Code, Label, CostCenter, Retries, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
