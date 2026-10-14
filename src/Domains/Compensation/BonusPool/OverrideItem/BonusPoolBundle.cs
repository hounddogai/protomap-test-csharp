namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool bundle is in its life cycle.</summary>
public enum BonusPoolBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One bonus pool bundle, with what the planners need to act on it.</summary>
public sealed class BonusPoolBundle(string code, string label, string salaryBand, int batchSize, double ratio, string location, BonusPoolBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public int BatchSize { get; set; } = batchSize;
    public double Ratio { get; set; } = ratio;
    public string Location { get; set; } = location;
    public BonusPoolBundleStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool bundle in status planned.</summary>
    public static BonusPoolBundle NewBonusPoolBundle(string code, string label)
    {
        return new BonusPoolBundle(code, label, "", 0, 0.0, "", BonusPoolBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolBundleStatus next)
    {
        switch (Status)
        {
            case BonusPoolBundleStatus.Planned:
                switch (next)
                {
                    case BonusPoolBundleStatus.InProgress:
                    case BonusPoolBundleStatus.Completed:
                        return true;
                }
                return false;
            case BonusPoolBundleStatus.Scheduled:
                switch (next)
                {
                    case BonusPoolBundleStatus.InProgress:
                    case BonusPoolBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case BonusPoolBundleStatus.InProgress:
                switch (next)
                {
                    case BonusPoolBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case BonusPoolBundleStatus.Completed:
                switch (next)
                {
                    case BonusPoolBundleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"bonus pool bundle"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Ratio + (double)BatchSize, 2);
    }

    public BonusPoolBundle Copy()
    {
        return new BonusPoolBundle(Code, Label, SalaryBand, BatchSize, Ratio, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
