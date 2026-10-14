namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant agreement is in its life cycle.</summary>
public enum EquityGrantAgreementStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a equity grant agreement and its current status.</summary>
public sealed class EquityGrantAgreement(string code, string name, string segment, double score, string location, int leaveDays, double minimum, int capacity, string team, EquityGrantAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public double Score { get; set; } = score;
    public string Location { get; set; } = location;
    public int LeaveDays { get; set; } = leaveDays;
    public double Minimum { get; set; } = minimum;
    public int Capacity { get; set; } = capacity;
    public string Team { get; set; } = team;
    public EquityGrantAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant agreement in status open.</summary>
    public static EquityGrantAgreement NewEquityGrantAgreement(string code, string name)
    {
        return new EquityGrantAgreement(code, name, "", 0.0, "", 0, 0.0, 0, "", EquityGrantAgreementStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantAgreementStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantAgreementStatus next)
    {
        switch (Status)
        {
            case EquityGrantAgreementStatus.Open:
                return next == EquityGrantAgreementStatus.Resolved;
            case EquityGrantAgreementStatus.InReview:
                switch (next)
                {
                    case EquityGrantAgreementStatus.Reopened:
                        return true;
                }
                return false;
            case EquityGrantAgreementStatus.Resolved:
                return next == EquityGrantAgreementStatus.Reopened || next == EquityGrantAgreementStatus.Archived;
            case EquityGrantAgreementStatus.Reopened:
                return next == EquityGrantAgreementStatus.Archived;
        }
        return false;
    }

    public void MoveTo(EquityGrantAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Minimum - (double)Capacity, 2);
    }

    public EquityGrantAgreement Copy()
    {
        return new EquityGrantAgreement(
            Code,
            Name,
            Segment,
            Score,
            Location,
            LeaveDays,
            Minimum,
            Capacity,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
