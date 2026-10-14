namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant audit is in its life cycle.</summary>
public enum EquityGrantAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One equity grant audit, with what the planners need to act on it.</summary>
public sealed class EquityGrantAudit(string code, string title, string source, string name, double maximum, string country, EquityGrantAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string Country { get; set; } = country;
    public EquityGrantAuditStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant audit in status draft.</summary>
    public static EquityGrantAudit NewEquityGrantAudit(string code, string title)
    {
        return new EquityGrantAudit(code, title, "", "", 0.0, "", EquityGrantAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantAuditStatus next)
    {
        switch (Status)
        {
            case EquityGrantAuditStatus.Draft:
                switch (next)
                {
                    case EquityGrantAuditStatus.Active:
                    case EquityGrantAuditStatus.Suspended:
                        return true;
                }
                return false;
            case EquityGrantAuditStatus.Pending:
                return next == EquityGrantAuditStatus.Active || next == EquityGrantAuditStatus.Closed;
            case EquityGrantAuditStatus.Active:
                return next == EquityGrantAuditStatus.Suspended || next == EquityGrantAuditStatus.Closed;
            case EquityGrantAuditStatus.Suspended:
                switch (next)
                {
                    case EquityGrantAuditStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"equity grant audit"} {Code}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public EquityGrantAudit Copy()
    {
        return new EquityGrantAudit(Code, Title, Source, Name, Maximum, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
