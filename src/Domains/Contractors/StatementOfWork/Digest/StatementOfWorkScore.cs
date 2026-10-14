namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work score is in its life cycle.</summary>
public enum StatementOfWorkScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One statement of work score, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkScore(string code, string name, string unit, string notes, double discount, int limit, string location, string segment, StatementOfWorkScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public string Notes { get; set; } = notes;
    public double Discount { get; set; } = discount;
    public int Limit { get; set; } = limit;
    public string Location { get; set; } = location;
    public string Segment { get; set; } = segment;
    public StatementOfWorkScoreStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work score in status requested.</summary>
    public static StatementOfWorkScore NewStatementOfWorkScore(string code, string name)
    {
        return new StatementOfWorkScore(code, name, "", "", 0.0, 0, "", "", StatementOfWorkScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkScoreStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkScoreStatus.Requested:
                switch (next)
                {
                    case StatementOfWorkScoreStatus.Granted:
                    case StatementOfWorkScoreStatus.Revoked:
                        return true;
                }
                return false;
            case StatementOfWorkScoreStatus.Granted:
                return next == StatementOfWorkScoreStatus.Expired;
            case StatementOfWorkScoreStatus.Revoked:
                return next == StatementOfWorkScoreStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Limit + Discount, 2);
    }

    public StatementOfWorkScore Copy()
    {
        return new StatementOfWorkScore(Code, Name, Unit, Notes, Discount, Limit, Location, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
