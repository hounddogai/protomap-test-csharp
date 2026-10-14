namespace Acme.Workforce.Workplace.AccessBadge.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge score is in its life cycle.</summary>
public enum AccessBadgeScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a access badge score and its current status.</summary>
public sealed class AccessBadgeScore(string code, string name, int graceDays, double cost, List<string> tags, string summary, double score, bool pinned, AccessBadgeScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public double Cost { get; set; } = cost;
    public List<string> Tags { get; set; } = tags;
    public string Summary { get; set; } = summary;
    public double Score { get; set; } = score;
    public bool Pinned { get; set; } = pinned;
    public AccessBadgeScoreStatus Status { get; set; } = status;

    /// <summary>Starts a access badge score in status new.</summary>
    public static AccessBadgeScore NewAccessBadgeScore(string code, string name)
    {
        return new AccessBadgeScore(
            code,
            name,
            0,
            0.0,
            new List<string>(),
            "",
            0.0,
            false,
            AccessBadgeScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeScoreStatus next)
    {
        switch (Status)
        {
            case AccessBadgeScoreStatus.New:
                switch (next)
                {
                    case AccessBadgeScoreStatus.Queued:
                    case AccessBadgeScoreStatus.Done:
                        return true;
                }
                return false;
            case AccessBadgeScoreStatus.Queued:
                switch (next)
                {
                    case AccessBadgeScoreStatus.Done:
                        return true;
                }
                return false;
            case AccessBadgeScoreStatus.Running:
                switch (next)
                {
                    case AccessBadgeScoreStatus.Done:
                    case AccessBadgeScoreStatus.Failed:
                        return true;
                }
                return false;
            case AccessBadgeScoreStatus.Done:
                switch (next)
                {
                    case AccessBadgeScoreStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveLoad()
    {
        return Math.Round(Score * Cost, 2);
    }

    public AccessBadgeScore Copy()
    {
        return new AccessBadgeScore(Code, Name, GraceDays, Cost, Tags, Summary, Score, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
