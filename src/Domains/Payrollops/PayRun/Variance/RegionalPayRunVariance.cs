namespace Acme.Workforce.Payrollops.PayRun.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional pay run variance is in its life cycle.</summary>
public enum RegionalPayRunVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a regional pay run variance and its current status.</summary>
public sealed class RegionalPayRunVariance(string code, string label, int batchSize, bool active, string country, RegionalPayRunVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public bool Active { get; set; } = active;
    public string Country { get; set; } = country;
    public RegionalPayRunVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a regional pay run variance in status draft.</summary>
    public static RegionalPayRunVariance NewRegionalPayRunVariance(string code, string label)
    {
        return new RegionalPayRunVariance(code, label, 0, false, "", RegionalPayRunVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPayRunVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPayRunVarianceStatus next)
    {
        switch (Status)
        {
            case RegionalPayRunVarianceStatus.Draft:
                return next == RegionalPayRunVarianceStatus.Active;
            case RegionalPayRunVarianceStatus.Pending:
                return next == RegionalPayRunVarianceStatus.Suspended;
            case RegionalPayRunVarianceStatus.Active:
                return next == RegionalPayRunVarianceStatus.Suspended || next == RegionalPayRunVarianceStatus.Closed;
            case RegionalPayRunVarianceStatus.Suspended:
                switch (next)
                {
                    case RegionalPayRunVarianceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalPayRunVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional pay run variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public RegionalPayRunVariance Copy()
    {
        return new RegionalPayRunVariance(Code, Label, BatchSize, Active, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
