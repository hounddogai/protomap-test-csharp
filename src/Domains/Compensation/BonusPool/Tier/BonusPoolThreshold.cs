namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool threshold is in its life cycle.</summary>
public enum BonusPoolThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A bonus pool threshold as the bonus pool team keeps it.</summary>
public sealed class BonusPoolThreshold(string code, string label, string description, double ratio, double discount, string contactEmail, BonusPoolThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public double Ratio { get; set; } = ratio;
    public double Discount { get; set; } = discount;
    public string ContactEmail { get; set; } = contactEmail;
    public BonusPoolThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool threshold in status requested.</summary>
    public static BonusPoolThreshold NewBonusPoolThreshold(string code, string label)
    {
        return new BonusPoolThreshold(code, label, "", 0.0, 0.0, "", BonusPoolThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolThresholdStatus next)
    {
        switch (Status)
        {
            case BonusPoolThresholdStatus.Requested:
                switch (next)
                {
                    case BonusPoolThresholdStatus.Expired:
                        return true;
                }
                return false;
            case BonusPoolThresholdStatus.Granted:
                switch (next)
                {
                    case BonusPoolThresholdStatus.Revoked:
                    case BonusPoolThresholdStatus.Expired:
                        return true;
                }
                return false;
            case BonusPoolThresholdStatus.Revoked:
                switch (next)
                {
                    case BonusPoolThresholdStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Discount + Ratio, 2);
    }

    public BonusPoolThreshold Copy()
    {
        return new BonusPoolThreshold(Code, Label, Description, Ratio, Discount, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
