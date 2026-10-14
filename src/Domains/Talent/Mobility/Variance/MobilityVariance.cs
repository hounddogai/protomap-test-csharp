namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility variance is in its life cycle.</summary>
public enum MobilityVarianceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A mobility variance. Its code is unique within a tenant.</summary>
public sealed class MobilityVariance(string code, string title, int limit, string managerId, double rate, string team, MobilityVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public string ManagerId { get; set; } = managerId;
    public double Rate { get; set; } = rate;
    public string Team { get; set; } = team;
    public MobilityVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a mobility variance in status open.</summary>
    public static MobilityVariance NewMobilityVariance(string code, string title)
    {
        return new MobilityVariance(code, title, 0, "", 0.0, "", MobilityVarianceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityVarianceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityVarianceStatus next)
    {
        switch (Status)
        {
            case MobilityVarianceStatus.Open:
                switch (next)
                {
                    case MobilityVarianceStatus.Archived:
                        return true;
                }
                return false;
            case MobilityVarianceStatus.InReview:
                switch (next)
                {
                    case MobilityVarianceStatus.Resolved:
                        return true;
                }
                return false;
            case MobilityVarianceStatus.Resolved:
                return next == MobilityVarianceStatus.Reopened || next == MobilityVarianceStatus.Archived;
            case MobilityVarianceStatus.Reopened:
                switch (next)
                {
                    case MobilityVarianceStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"mobility variance"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Rate * (double)Limit, 2);
    }

    public MobilityVariance Copy()
    {
        return new MobilityVariance(Code, Title, Limit, ManagerId, Rate, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
