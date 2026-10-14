namespace Acme.Workforce.Engagement.TownHall.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive town hall score is in its life cycle.</summary>
public enum AdaptiveTownHallScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One adaptive town hall score, with what the planners need to act on it.</summary>
public sealed class AdaptiveTownHallScore(string code, string name, string phone, double fte, double discount, int durationMinutes, string salaryBand, AdaptiveTownHallScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public double Fte { get; set; } = fte;
    public double Discount { get; set; } = discount;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string SalaryBand { get; set; } = salaryBand;
    public AdaptiveTownHallScoreStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive town hall score in status new.</summary>
    public static AdaptiveTownHallScore NewAdaptiveTownHallScore(string code, string name)
    {
        return new AdaptiveTownHallScore(code, name, "", 0.0, 0.0, 0, "", AdaptiveTownHallScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTownHallScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTownHallScoreStatus next)
    {
        switch (Status)
        {
            case AdaptiveTownHallScoreStatus.New:
                switch (next)
                {
                    case AdaptiveTownHallScoreStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveTownHallScoreStatus.Queued:
                switch (next)
                {
                    case AdaptiveTownHallScoreStatus.Done:
                    case AdaptiveTownHallScoreStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveTownHallScoreStatus.Running:
                switch (next)
                {
                    case AdaptiveTownHallScoreStatus.Done:
                    case AdaptiveTownHallScoreStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveTownHallScoreStatus.Done:
                return next == AdaptiveTownHallScoreStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdaptiveTownHallScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive town hall score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Fte - (double)DurationMinutes, 2);
    }

    public AdaptiveTownHallScore Copy()
    {
        return new AdaptiveTownHallScore(Code, Name, Phone, Fte, Discount, DurationMinutes, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
