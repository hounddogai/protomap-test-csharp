namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal bonus pool request is in its life cycle.</summary>
public enum SeasonalBonusPoolRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a seasonal bonus pool request and its current status.</summary>
public sealed class SeasonalBonusPoolRequest(string code, string label, double score, double ceiling, string category, double fte, double rate, double minimum, string timezone, SeasonalBonusPoolRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public double Ceiling { get; set; } = ceiling;
    public string Category { get; set; } = category;
    public double Fte { get; set; } = fte;
    public double Rate { get; set; } = rate;
    public double Minimum { get; set; } = minimum;
    public string Timezone { get; set; } = timezone;
    public SeasonalBonusPoolRequestStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal bonus pool request in status new.</summary>
    public static SeasonalBonusPoolRequest NewSeasonalBonusPoolRequest(string code, string label)
    {
        return new SeasonalBonusPoolRequest(
            code,
            label,
            0.0,
            0.0,
            "",
            0.0,
            0.0,
            0.0,
            "",
            SeasonalBonusPoolRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalBonusPoolRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalBonusPoolRequestStatus next)
    {
        switch (Status)
        {
            case SeasonalBonusPoolRequestStatus.New:
                return next == SeasonalBonusPoolRequestStatus.Done;
            case SeasonalBonusPoolRequestStatus.Queued:
                return next == SeasonalBonusPoolRequestStatus.Done;
            case SeasonalBonusPoolRequestStatus.Running:
                switch (next)
                {
                    case SeasonalBonusPoolRequestStatus.Done:
                        return true;
                }
                return false;
            case SeasonalBonusPoolRequestStatus.Done:
                switch (next)
                {
                    case SeasonalBonusPoolRequestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalBonusPoolRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal bonus pool request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetExposure()
    {
        return Math.Round(Fte + Ceiling, 2);
    }

    public SeasonalBonusPoolRequest Copy()
    {
        return new SeasonalBonusPoolRequest(
            Code,
            Label,
            Score,
            Ceiling,
            Category,
            Fte,
            Rate,
            Minimum,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
