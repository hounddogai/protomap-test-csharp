namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate score is in its life cycle.</summary>
public enum RateScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A rate score as the rate team keeps it.</summary>
public sealed class RateScore(string code, string label, double cost, double fte, int priority, RateScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public double Fte { get; set; } = fte;
    public int Priority { get; set; } = priority;
    public RateScoreStatus Status { get; set; } = status;

    /// <summary>Starts a rate score in status new.</summary>
    public static RateScore NewRateScore(string code, string label)
    {
        return new RateScore(code, label, 0.0, 0.0, 0, RateScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateScoreStatus next)
    {
        switch (Status)
        {
            case RateScoreStatus.New:
                switch (next)
                {
                    case RateScoreStatus.Running:
                        return true;
                }
                return false;
            case RateScoreStatus.Queued:
                return next == RateScoreStatus.Running || next == RateScoreStatus.Done;
            case RateScoreStatus.Running:
                switch (next)
                {
                    case RateScoreStatus.Done:
                    case RateScoreStatus.Failed:
                        return true;
                }
                return false;
            case RateScoreStatus.Done:
                return next == RateScoreStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RateScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Priority + Fte, 2);
    }

    public RateScore Copy()
    {
        return new RateScore(Code, Label, Cost, Fte, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
