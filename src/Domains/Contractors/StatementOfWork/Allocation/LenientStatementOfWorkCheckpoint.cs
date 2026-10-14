namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient statement of work checkpoint is in its life cycle.</summary>
public enum LenientStatementOfWorkCheckpointStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One lenient statement of work checkpoint, with what the planners need to act on it.</summary>
public sealed class LenientStatementOfWorkCheckpoint(string code, string name, string notes, double margin, int limit, double rate, string contactEmail, double ratio, int version, LenientStatementOfWorkCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public double Margin { get; set; } = margin;
    public int Limit { get; set; } = limit;
    public double Rate { get; set; } = rate;
    public string ContactEmail { get; set; } = contactEmail;
    public double Ratio { get; set; } = ratio;
    public int Version { get; set; } = version;
    public LenientStatementOfWorkCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a lenient statement of work checkpoint in status draft.</summary>
    public static LenientStatementOfWorkCheckpoint NewLenientStatementOfWorkCheckpoint(string code, string name)
    {
        return new LenientStatementOfWorkCheckpoint(
            code,
            name,
            "",
            0.0,
            0,
            0.0,
            "",
            0.0,
            0,
            LenientStatementOfWorkCheckpointStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientStatementOfWorkCheckpointStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientStatementOfWorkCheckpointStatus next)
    {
        switch (Status)
        {
            case LenientStatementOfWorkCheckpointStatus.Draft:
                return next == LenientStatementOfWorkCheckpointStatus.Active || next == LenientStatementOfWorkCheckpointStatus.Closed;
            case LenientStatementOfWorkCheckpointStatus.Pending:
                return next == LenientStatementOfWorkCheckpointStatus.Active || next == LenientStatementOfWorkCheckpointStatus.Suspended;
            case LenientStatementOfWorkCheckpointStatus.Active:
                switch (next)
                {
                    case LenientStatementOfWorkCheckpointStatus.Suspended:
                    case LenientStatementOfWorkCheckpointStatus.Closed:
                        return true;
                }
                return false;
            case LenientStatementOfWorkCheckpointStatus.Suspended:
                switch (next)
                {
                    case LenientStatementOfWorkCheckpointStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientStatementOfWorkCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient statement of work checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetLoad()
    {
        return Math.Round(Ratio + (double)Version, 2);
    }

    public LenientStatementOfWorkCheckpoint Copy()
    {
        return new LenientStatementOfWorkCheckpoint(
            Code,
            Name,
            Notes,
            Margin,
            Limit,
            Rate,
            ContactEmail,
            Ratio,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
