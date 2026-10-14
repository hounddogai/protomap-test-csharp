namespace Acme.Workforce.Talent.Nomination.NominationReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global nomination bundle is in its life cycle.</summary>
public enum GlobalNominationBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One global nomination bundle, with what the planners need to act on it.</summary>
public sealed class GlobalNominationBundle(string code, string name, double score, double cost, string reviewer, GlobalNominationBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public double Cost { get; set; } = cost;
    public string Reviewer { get; set; } = reviewer;
    public GlobalNominationBundleStatus Status { get; set; } = status;

    /// <summary>Starts a global nomination bundle in status draft.</summary>
    public static GlobalNominationBundle NewGlobalNominationBundle(string code, string name)
    {
        return new GlobalNominationBundle(code, name, 0.0, 0.0, "", GlobalNominationBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalNominationBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalNominationBundleStatus next)
    {
        switch (Status)
        {
            case GlobalNominationBundleStatus.Draft:
                switch (next)
                {
                    case GlobalNominationBundleStatus.Pending:
                    case GlobalNominationBundleStatus.Closed:
                        return true;
                }
                return false;
            case GlobalNominationBundleStatus.Pending:
                switch (next)
                {
                    case GlobalNominationBundleStatus.Closed:
                        return true;
                }
                return false;
            case GlobalNominationBundleStatus.Active:
                switch (next)
                {
                    case GlobalNominationBundleStatus.Closed:
                        return true;
                }
                return false;
            case GlobalNominationBundleStatus.Suspended:
                return next == GlobalNominationBundleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(GlobalNominationBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global nomination bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetTotal()
    {
        return Math.Round(Score - Cost, 2);
    }

    public GlobalNominationBundle Copy()
    {
        return new GlobalNominationBundle(Code, Name, Score, Cost, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
