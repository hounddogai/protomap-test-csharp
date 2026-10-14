namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted refresh score is in its life cycle.</summary>
public enum WeightedRefreshScoreStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A weighted refresh score as the refresh team keeps it.</summary>
public sealed class WeightedRefreshScore(string code, string label, string ownerId, string badge, double ceiling, string costCenter, int startsAt, bool visible, WeightedRefreshScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string Badge { get; set; } = badge;
    public double Ceiling { get; set; } = ceiling;
    public string CostCenter { get; set; } = costCenter;
    public int StartsAt { get; set; } = startsAt;
    public bool Visible { get; set; } = visible;
    public WeightedRefreshScoreStatus Status { get; set; } = status;

    /// <summary>Starts a weighted refresh score in status planned.</summary>
    public static WeightedRefreshScore NewWeightedRefreshScore(string code, string label)
    {
        return new WeightedRefreshScore(code, label, "", "", 0.0, "", 0, false, WeightedRefreshScoreStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedRefreshScoreStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedRefreshScoreStatus next)
    {
        switch (Status)
        {
            case WeightedRefreshScoreStatus.Planned:
                switch (next)
                {
                    case WeightedRefreshScoreStatus.Scheduled:
                        return true;
                }
                return false;
            case WeightedRefreshScoreStatus.Scheduled:
                return next == WeightedRefreshScoreStatus.InProgress || next == WeightedRefreshScoreStatus.Cancelled;
            case WeightedRefreshScoreStatus.InProgress:
                switch (next)
                {
                    case WeightedRefreshScoreStatus.Cancelled:
                        return true;
                }
                return false;
            case WeightedRefreshScoreStatus.Completed:
                switch (next)
                {
                    case WeightedRefreshScoreStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedRefreshScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted refresh score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedValue()
    {
        return Math.Round(Ceiling + (double)StartsAt, 2);
    }

    public WeightedRefreshScore Copy()
    {
        return new WeightedRefreshScore(Code, Label, OwnerId, Badge, Ceiling, CostCenter, StartsAt, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
