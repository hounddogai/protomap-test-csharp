namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall rule is in its life cycle.</summary>
public enum TownHallRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One town hall rule, with what the planners need to act on it.</summary>
public sealed class TownHallRule(string code, string name, double rating, double ratio, string timezone, string jobCode, double score, TownHallRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public double Ratio { get; set; } = ratio;
    public string Timezone { get; set; } = timezone;
    public string JobCode { get; set; } = jobCode;
    public double Score { get; set; } = score;
    public TownHallRuleStatus Status { get; set; } = status;

    /// <summary>Starts a town hall rule in status new.</summary>
    public static TownHallRule NewTownHallRule(string code, string name)
    {
        return new TownHallRule(code, name, 0.0, 0.0, "", "", 0.0, TownHallRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallRuleStatus next)
    {
        switch (Status)
        {
            case TownHallRuleStatus.New:
                switch (next)
                {
                    case TownHallRuleStatus.Queued:
                    case TownHallRuleStatus.Failed:
                        return true;
                }
                return false;
            case TownHallRuleStatus.Queued:
                return next == TownHallRuleStatus.Done;
            case TownHallRuleStatus.Running:
                switch (next)
                {
                    case TownHallRuleStatus.Done:
                    case TownHallRuleStatus.Failed:
                        return true;
                }
                return false;
            case TownHallRuleStatus.Done:
                switch (next)
                {
                    case TownHallRuleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetValue()
    {
        return Math.Round(Rating * Score, 2);
    }

    public TownHallRule Copy()
    {
        return new TownHallRule(Code, Name, Rating, Ratio, Timezone, JobCode, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
