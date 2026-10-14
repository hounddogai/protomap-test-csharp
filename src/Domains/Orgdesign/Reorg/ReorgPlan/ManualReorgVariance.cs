namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual reorg variance is in its life cycle.</summary>
public enum ManualReorgVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a manual reorg variance and its current status.</summary>
public sealed class ManualReorgVariance(string code, string name, double discount, double maximum, double fte, string costCenter, int graceDays, int limit, string title, int windowDays, ManualReorgVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public double Maximum { get; set; } = maximum;
    public double Fte { get; set; } = fte;
    public string CostCenter { get; set; } = costCenter;
    public int GraceDays { get; set; } = graceDays;
    public int Limit { get; set; } = limit;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public ManualReorgVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a manual reorg variance in status draft.</summary>
    public static ManualReorgVariance NewManualReorgVariance(string code, string name)
    {
        return new ManualReorgVariance(code, name, 0.0, 0.0, 0.0, "", 0, 0, "", 0, ManualReorgVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualReorgVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualReorgVarianceStatus next)
    {
        switch (Status)
        {
            case ManualReorgVarianceStatus.Draft:
                switch (next)
                {
                    case ManualReorgVarianceStatus.Pending:
                    case ManualReorgVarianceStatus.Suspended:
                        return true;
                }
                return false;
            case ManualReorgVarianceStatus.Pending:
                return next == ManualReorgVarianceStatus.Active || next == ManualReorgVarianceStatus.Suspended;
            case ManualReorgVarianceStatus.Active:
                return next == ManualReorgVarianceStatus.Suspended;
            case ManualReorgVarianceStatus.Suspended:
                switch (next)
                {
                    case ManualReorgVarianceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualReorgVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual reorg variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Discount - Maximum, 2);
    }

    public ManualReorgVariance Copy()
    {
        return new ManualReorgVariance(
            Code,
            Name,
            Discount,
            Maximum,
            Fte,
            CostCenter,
            GraceDays,
            Limit,
            Title,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
