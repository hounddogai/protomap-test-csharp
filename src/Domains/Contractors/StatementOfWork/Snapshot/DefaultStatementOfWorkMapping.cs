namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default statement of work mapping is in its life cycle.</summary>
public enum DefaultStatementOfWorkMappingStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A default statement of work mapping as the statement of work team keeps it.</summary>
public sealed class DefaultStatementOfWorkMapping(string code, string label, bool pinned, string source, double weight, DefaultStatementOfWorkMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public string Source { get; set; } = source;
    public double Weight { get; set; } = weight;
    public DefaultStatementOfWorkMappingStatus Status { get; set; } = status;

    /// <summary>Starts a default statement of work mapping in status draft.</summary>
    public static DefaultStatementOfWorkMapping NewDefaultStatementOfWorkMapping(string code, string label)
    {
        return new DefaultStatementOfWorkMapping(
            code,
            label,
            false,
            "",
            0.0,
            DefaultStatementOfWorkMappingStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultStatementOfWorkMappingStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultStatementOfWorkMappingStatus next)
    {
        switch (Status)
        {
            case DefaultStatementOfWorkMappingStatus.Draft:
                switch (next)
                {
                    case DefaultStatementOfWorkMappingStatus.Suspended:
                        return true;
                }
                return false;
            case DefaultStatementOfWorkMappingStatus.Pending:
                switch (next)
                {
                    case DefaultStatementOfWorkMappingStatus.Suspended:
                        return true;
                }
                return false;
            case DefaultStatementOfWorkMappingStatus.Active:
                switch (next)
                {
                    case DefaultStatementOfWorkMappingStatus.Suspended:
                    case DefaultStatementOfWorkMappingStatus.Closed:
                        return true;
                }
                return false;
            case DefaultStatementOfWorkMappingStatus.Suspended:
                return next == DefaultStatementOfWorkMappingStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DefaultStatementOfWorkMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default statement of work mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"default statement of work mapping"} {Code}";
    }

    public double MeasuredValue()
    {
        return Weight;
    }

    public DefaultStatementOfWorkMapping Copy()
    {
        return new DefaultStatementOfWorkMapping(Code, Label, Pinned, Source, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
