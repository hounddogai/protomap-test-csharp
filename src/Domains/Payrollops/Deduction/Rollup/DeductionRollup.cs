namespace Acme.Workforce.Payrollops.Deduction.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction rollup is in its life cycle.</summary>
public enum DeductionRollupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A deduction rollup. Its code is unique within a tenant.</summary>
public sealed class DeductionRollup(string code, string name, int sequence, double rate, int retries, string summary, double threshold, DeductionRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public double Rate { get; set; } = rate;
    public int Retries { get; set; } = retries;
    public string Summary { get; set; } = summary;
    public double Threshold { get; set; } = threshold;
    public DeductionRollupStatus Status { get; set; } = status;

    /// <summary>Starts a deduction rollup in status draft.</summary>
    public static DeductionRollup NewDeductionRollup(string code, string name)
    {
        return new DeductionRollup(code, name, 0, 0.0, 0, "", 0.0, DeductionRollupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionRollupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionRollupStatus next)
    {
        switch (Status)
        {
            case DeductionRollupStatus.Draft:
                switch (next)
                {
                    case DeductionRollupStatus.Suspended:
                    case DeductionRollupStatus.Closed:
                        return true;
                }
                return false;
            case DeductionRollupStatus.Pending:
                return next == DeductionRollupStatus.Closed;
            case DeductionRollupStatus.Active:
                return next == DeductionRollupStatus.Suspended || next == DeductionRollupStatus.Closed;
            case DeductionRollupStatus.Suspended:
                return next == DeductionRollupStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DeductionRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Sequence * Rate, 2);
    }

    public DeductionRollup Copy()
    {
        return new DeductionRollup(Code, Name, Sequence, Rate, Retries, Summary, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
