namespace Acme.Workforce.Talent.Mobility.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility summary is in its life cycle.</summary>
public enum MobilitySummaryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One mobility summary, with what the planners need to act on it.</summary>
public sealed class MobilitySummary(string code, string title, string contactEmail, string team, double budget, bool pinned, int tenureMonths, string department, MobilitySummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public string Team { get; set; } = team;
    public double Budget { get; set; } = budget;
    public bool Pinned { get; set; } = pinned;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Department { get; set; } = department;
    public MobilitySummaryStatus Status { get; set; } = status;

    /// <summary>Starts a mobility summary in status open.</summary>
    public static MobilitySummary NewMobilitySummary(string code, string title)
    {
        return new MobilitySummary(code, title, "", "", 0.0, false, 0, "", MobilitySummaryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilitySummaryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilitySummaryStatus next)
    {
        switch (Status)
        {
            case MobilitySummaryStatus.Open:
                return next == MobilitySummaryStatus.Resolved || next == MobilitySummaryStatus.Archived;
            case MobilitySummaryStatus.InReview:
                switch (next)
                {
                    case MobilitySummaryStatus.Reopened:
                    case MobilitySummaryStatus.Archived:
                        return true;
                }
                return false;
            case MobilitySummaryStatus.Resolved:
                return next == MobilitySummaryStatus.Reopened || next == MobilitySummaryStatus.Archived;
            case MobilitySummaryStatus.Reopened:
                switch (next)
                {
                    case MobilitySummaryStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilitySummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)TenureMonths - Budget, 2);
    }

    public MobilitySummary Copy()
    {
        return new MobilitySummary(
            Code,
            Title,
            ContactEmail,
            Team,
            Budget,
            Pinned,
            TenureMonths,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
