namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool variance is in its life cycle.</summary>
public enum BonusPoolVarianceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A bonus pool variance. Its code is unique within a tenant.</summary>
public sealed class BonusPoolVariance(string code, string label, string managerId, double weight, string channel, bool pinned, BonusPoolVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public double Weight { get; set; } = weight;
    public string Channel { get; set; } = channel;
    public bool Pinned { get; set; } = pinned;
    public BonusPoolVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool variance in status requested.</summary>
    public static BonusPoolVariance NewBonusPoolVariance(string code, string label)
    {
        return new BonusPoolVariance(code, label, "", 0.0, "", false, BonusPoolVarianceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolVarianceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolVarianceStatus next)
    {
        switch (Status)
        {
            case BonusPoolVarianceStatus.Requested:
                switch (next)
                {
                    case BonusPoolVarianceStatus.Revoked:
                        return true;
                }
                return false;
            case BonusPoolVarianceStatus.Granted:
                return next == BonusPoolVarianceStatus.Expired;
            case BonusPoolVarianceStatus.Revoked:
                switch (next)
                {
                    case BonusPoolVarianceStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Weight;
    }

    public BonusPoolVariance Copy()
    {
        return new BonusPoolVariance(Code, Label, ManagerId, Weight, Channel, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
