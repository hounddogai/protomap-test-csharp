namespace Acme.Workforce.Assets.Refresh.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal refresh allocation is in its life cycle.</summary>
public enum SeasonalRefreshAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A seasonal refresh allocation. Its code is unique within a tenant.</summary>
public sealed class SeasonalRefreshAllocation(string code, string name, bool archived, double discount, double hours, int priority, string grade, int startsAt, string employeeId, int version, string locale, SeasonalRefreshAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public double Discount { get; set; } = discount;
    public double Hours { get; set; } = hours;
    public int Priority { get; set; } = priority;
    public string Grade { get; set; } = grade;
    public int StartsAt { get; set; } = startsAt;
    public string EmployeeId { get; set; } = employeeId;
    public int Version { get; set; } = version;
    public string Locale { get; set; } = locale;
    public SeasonalRefreshAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal refresh allocation in status requested.</summary>
    public static SeasonalRefreshAllocation NewSeasonalRefreshAllocation(string code, string name)
    {
        return new SeasonalRefreshAllocation(
            code,
            name,
            false,
            0.0,
            0.0,
            0,
            "",
            0,
            "",
            0,
            "",
            SeasonalRefreshAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalRefreshAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalRefreshAllocationStatus next)
    {
        switch (Status)
        {
            case SeasonalRefreshAllocationStatus.Requested:
                switch (next)
                {
                    case SeasonalRefreshAllocationStatus.Granted:
                        return true;
                }
                return false;
            case SeasonalRefreshAllocationStatus.Granted:
                return next == SeasonalRefreshAllocationStatus.Revoked || next == SeasonalRefreshAllocationStatus.Expired;
            case SeasonalRefreshAllocationStatus.Revoked:
                return next == SeasonalRefreshAllocationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SeasonalRefreshAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal refresh allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"seasonal refresh allocation"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Hours + Discount, 2);
    }

    public SeasonalRefreshAllocation Copy()
    {
        return new SeasonalRefreshAllocation(
            Code,
            Name,
            Archived,
            Discount,
            Hours,
            Priority,
            Grade,
            StartsAt,
            EmployeeId,
            Version,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
