namespace Acme.Workforce.Orgdesign.Position.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position rollup is in its life cycle.</summary>
public enum PositionRollupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A position rollup as the position team keeps it.</summary>
public sealed class PositionRollup(string code, string title, int quantity, string skill, string source, string timezone, string grade, double maximum, int retries, PositionRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string Skill { get; set; } = skill;
    public string Source { get; set; } = source;
    public string Timezone { get; set; } = timezone;
    public string Grade { get; set; } = grade;
    public double Maximum { get; set; } = maximum;
    public int Retries { get; set; } = retries;
    public PositionRollupStatus Status { get; set; } = status;

    /// <summary>Starts a position rollup in status requested.</summary>
    public static PositionRollup NewPositionRollup(string code, string title)
    {
        return new PositionRollup(code, title, 0, "", "", "", "", 0.0, 0, PositionRollupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionRollupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionRollupStatus next)
    {
        switch (Status)
        {
            case PositionRollupStatus.Requested:
                return next == PositionRollupStatus.Granted;
            case PositionRollupStatus.Granted:
                switch (next)
                {
                    case PositionRollupStatus.Expired:
                        return true;
                }
                return false;
            case PositionRollupStatus.Revoked:
                return next == PositionRollupStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PositionRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Retries - Maximum, 2);
    }

    public PositionRollup Copy()
    {
        return new PositionRollup(Code, Title, Quantity, Skill, Source, Timezone, Grade, Maximum, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
