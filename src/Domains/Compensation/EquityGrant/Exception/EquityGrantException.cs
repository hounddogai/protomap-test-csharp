namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant exception is in its life cycle.</summary>
public enum EquityGrantExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A equity grant exception as the equity grant team keeps it.</summary>
public sealed class EquityGrantException(string code, string name, string title, string label, double budget, double ceiling, string externalRef, string currency, int sequence, string source, Dictionary<string, string> attributes, EquityGrantExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public double Ceiling { get; set; } = ceiling;
    public string ExternalRef { get; set; } = externalRef;
    public string Currency { get; set; } = currency;
    public int Sequence { get; set; } = sequence;
    public string Source { get; set; } = source;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public EquityGrantExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant exception in status planned.</summary>
    public static EquityGrantException NewEquityGrantException(string code, string name)
    {
        return new EquityGrantException(
            code,
            name,
            "",
            "",
            0.0,
            0.0,
            "",
            "",
            0,
            "",
            new Dictionary<string, string>(),
            EquityGrantExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantExceptionStatus next)
    {
        switch (Status)
        {
            case EquityGrantExceptionStatus.Planned:
                switch (next)
                {
                    case EquityGrantExceptionStatus.InProgress:
                        return true;
                }
                return false;
            case EquityGrantExceptionStatus.Scheduled:
                return next == EquityGrantExceptionStatus.InProgress;
            case EquityGrantExceptionStatus.InProgress:
                switch (next)
                {
                    case EquityGrantExceptionStatus.Completed:
                    case EquityGrantExceptionStatus.Cancelled:
                        return true;
                }
                return false;
            case EquityGrantExceptionStatus.Completed:
                return next == EquityGrantExceptionStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(EquityGrantExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
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

    public double ProjectedValue()
    {
        return Math.Round(Budget * Ceiling, 2);
    }

    public EquityGrantException Copy()
    {
        return new EquityGrantException(
            Code,
            Name,
            Title,
            Label,
            Budget,
            Ceiling,
            ExternalRef,
            Currency,
            Sequence,
            Source,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
