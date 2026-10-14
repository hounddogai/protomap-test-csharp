namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered pool exception is in its life cycle.</summary>
public enum TieredPoolExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A tiered pool exception. Its code is unique within a tenant.</summary>
public sealed class TieredPoolException(string code, string name, int batchSize, bool archived, double rate, string skill, string department, TieredPoolExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public bool Archived { get; set; } = archived;
    public double Rate { get; set; } = rate;
    public string Skill { get; set; } = skill;
    public string Department { get; set; } = department;
    public TieredPoolExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a tiered pool exception in status proposed.</summary>
    public static TieredPoolException NewTieredPoolException(string code, string name)
    {
        return new TieredPoolException(code, name, 0, false, 0.0, "", "", TieredPoolExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPoolExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPoolExceptionStatus next)
    {
        switch (Status)
        {
            case TieredPoolExceptionStatus.Proposed:
                switch (next)
                {
                    case TieredPoolExceptionStatus.Approved:
                    case TieredPoolExceptionStatus.Rejected:
                        return true;
                }
                return false;
            case TieredPoolExceptionStatus.Approved:
                return next == TieredPoolExceptionStatus.Rejected || next == TieredPoolExceptionStatus.Withdrawn;
            case TieredPoolExceptionStatus.Rejected:
                return next == TieredPoolExceptionStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TieredPoolExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered pool exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedValue()
    {
        return Math.Round(Rate + (double)BatchSize, 2);
    }

    public TieredPoolException Copy()
    {
        return new TieredPoolException(Code, Name, BatchSize, Archived, Rate, Skill, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
