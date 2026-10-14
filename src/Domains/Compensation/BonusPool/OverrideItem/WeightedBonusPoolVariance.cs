namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted bonus pool variance is in its life cycle.</summary>
public enum WeightedBonusPoolVarianceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted bonus pool variance. Its code is unique within a tenant.</summary>
public sealed class WeightedBonusPoolVariance(string code, string title, double hours, int graceDays, double minimum, string checksum, WeightedBonusPoolVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public int GraceDays { get; set; } = graceDays;
    public double Minimum { get; set; } = minimum;
    public string Checksum { get; set; } = checksum;
    public WeightedBonusPoolVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a weighted bonus pool variance in status proposed.</summary>
    public static WeightedBonusPoolVariance NewWeightedBonusPoolVariance(string code, string title)
    {
        return new WeightedBonusPoolVariance(code, title, 0.0, 0, 0.0, "", WeightedBonusPoolVarianceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedBonusPoolVarianceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedBonusPoolVarianceStatus next)
    {
        switch (Status)
        {
            case WeightedBonusPoolVarianceStatus.Proposed:
                switch (next)
                {
                    case WeightedBonusPoolVarianceStatus.Rejected:
                    case WeightedBonusPoolVarianceStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedBonusPoolVarianceStatus.Approved:
                return next == WeightedBonusPoolVarianceStatus.Rejected;
            case WeightedBonusPoolVarianceStatus.Rejected:
                return next == WeightedBonusPoolVarianceStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedBonusPoolVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted bonus pool variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Minimum - (double)GraceDays, 2);
    }

    public WeightedBonusPoolVariance Copy()
    {
        return new WeightedBonusPoolVariance(Code, Title, Hours, GraceDays, Minimum, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
