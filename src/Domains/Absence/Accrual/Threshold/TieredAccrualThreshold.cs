namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered accrual threshold is in its life cycle.</summary>
public enum TieredAccrualThresholdStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A tiered accrual threshold as the accrual team keeps it.</summary>
public sealed class TieredAccrualThreshold(string code, string name, double ratio, int limit, string segment, string title, double fte, TieredAccrualThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public int Limit { get; set; } = limit;
    public string Segment { get; set; } = segment;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public TieredAccrualThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a tiered accrual threshold in status created.</summary>
    public static TieredAccrualThreshold NewTieredAccrualThreshold(string code, string name)
    {
        return new TieredAccrualThreshold(code, name, 0.0, 0, "", "", 0.0, TieredAccrualThresholdStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredAccrualThresholdStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredAccrualThresholdStatus next)
    {
        switch (Status)
        {
            case TieredAccrualThresholdStatus.Created:
                switch (next)
                {
                    case TieredAccrualThresholdStatus.Validated:
                    case TieredAccrualThresholdStatus.Retired:
                        return true;
                }
                return false;
            case TieredAccrualThresholdStatus.Validated:
                switch (next)
                {
                    case TieredAccrualThresholdStatus.Published:
                        return true;
                }
                return false;
            case TieredAccrualThresholdStatus.Published:
                switch (next)
                {
                    case TieredAccrualThresholdStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredAccrualThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered accrual threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ratio + (double)Limit, 2);
    }

    public TieredAccrualThreshold Copy()
    {
        return new TieredAccrualThreshold(Code, Name, Ratio, Limit, Segment, Title, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
