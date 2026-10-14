namespace Acme.Workforce.Assets.Refresh.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered refresh score is in its life cycle.</summary>
public enum TieredRefreshScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a tiered refresh score and its current status.</summary>
public sealed class TieredRefreshScore(string code, string name, string label, bool approved, double threshold, int quantity, double ceiling, int graceDays, TieredRefreshScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public double Threshold { get; set; } = threshold;
    public int Quantity { get; set; } = quantity;
    public double Ceiling { get; set; } = ceiling;
    public int GraceDays { get; set; } = graceDays;
    public TieredRefreshScoreStatus Status { get; set; } = status;

    /// <summary>Starts a tiered refresh score in status requested.</summary>
    public static TieredRefreshScore NewTieredRefreshScore(string code, string name)
    {
        return new TieredRefreshScore(code, name, "", false, 0.0, 0, 0.0, 0, TieredRefreshScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredRefreshScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredRefreshScoreStatus next)
    {
        switch (Status)
        {
            case TieredRefreshScoreStatus.Requested:
                switch (next)
                {
                    case TieredRefreshScoreStatus.Granted:
                    case TieredRefreshScoreStatus.Expired:
                        return true;
                }
                return false;
            case TieredRefreshScoreStatus.Granted:
                return next == TieredRefreshScoreStatus.Revoked || next == TieredRefreshScoreStatus.Expired;
            case TieredRefreshScoreStatus.Revoked:
                switch (next)
                {
                    case TieredRefreshScoreStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredRefreshScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered refresh score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Quantity * Ceiling, 2);
    }

    public TieredRefreshScore Copy()
    {
        return new TieredRefreshScore(Code, Name, Label, Approved, Threshold, Quantity, Ceiling, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
