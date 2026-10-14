namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model score is in its life cycle.</summary>
public enum CostModelScoreStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One cost model score, with what the planners need to act on it.</summary>
public sealed class CostModelScore(string code, string title, int quantity, string badge, int windowDays, string region, CostModelScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string Badge { get; set; } = badge;
    public int WindowDays { get; set; } = windowDays;
    public string Region { get; set; } = region;
    public CostModelScoreStatus Status { get; set; } = status;

    /// <summary>Starts a cost model score in status proposed.</summary>
    public static CostModelScore NewCostModelScore(string code, string title)
    {
        return new CostModelScore(code, title, 0, "", 0, "", CostModelScoreStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelScoreStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelScoreStatus next)
    {
        switch (Status)
        {
            case CostModelScoreStatus.Proposed:
                switch (next)
                {
                    case CostModelScoreStatus.Rejected:
                        return true;
                }
                return false;
            case CostModelScoreStatus.Approved:
                switch (next)
                {
                    case CostModelScoreStatus.Rejected:
                    case CostModelScoreStatus.Withdrawn:
                        return true;
                }
                return false;
            case CostModelScoreStatus.Rejected:
                return next == CostModelScoreStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CostModelScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)WindowDays - (double)Quantity, 2);
    }

    public CostModelScore Copy()
    {
        return new CostModelScore(Code, Title, Quantity, Badge, WindowDays, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
