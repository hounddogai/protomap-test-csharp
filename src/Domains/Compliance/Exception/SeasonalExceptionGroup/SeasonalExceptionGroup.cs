namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal exception group is in its life cycle.</summary>
public enum SeasonalExceptionGroupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A seasonal exception group. Its code is unique within a tenant.</summary>
public sealed class SeasonalExceptionGroup(string code, string label, double minimum, string managerId, double discount, int graceDays, int batchSize, double ceiling, SeasonalExceptionGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public string ManagerId { get; set; } = managerId;
    public double Discount { get; set; } = discount;
    public int GraceDays { get; set; } = graceDays;
    public int BatchSize { get; set; } = batchSize;
    public double Ceiling { get; set; } = ceiling;
    public SeasonalExceptionGroupStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal exception group in status draft.</summary>
    public static SeasonalExceptionGroup NewSeasonalExceptionGroup(string code, string label)
    {
        return new SeasonalExceptionGroup(code, label, 0.0, "", 0.0, 0, 0, 0.0, SeasonalExceptionGroupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalExceptionGroupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalExceptionGroupStatus next)
    {
        switch (Status)
        {
            case SeasonalExceptionGroupStatus.Draft:
                switch (next)
                {
                    case SeasonalExceptionGroupStatus.Pending:
                    case SeasonalExceptionGroupStatus.Closed:
                        return true;
                }
                return false;
            case SeasonalExceptionGroupStatus.Pending:
                switch (next)
                {
                    case SeasonalExceptionGroupStatus.Closed:
                        return true;
                }
                return false;
            case SeasonalExceptionGroupStatus.Active:
                return next == SeasonalExceptionGroupStatus.Suspended;
            case SeasonalExceptionGroupStatus.Suspended:
                return next == SeasonalExceptionGroupStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SeasonalExceptionGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal exception group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)GraceDays - Minimum, 2);
    }

    public SeasonalExceptionGroup Copy()
    {
        return new SeasonalExceptionGroup(
            Code,
            Label,
            Minimum,
            ManagerId,
            Discount,
            GraceDays,
            BatchSize,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
