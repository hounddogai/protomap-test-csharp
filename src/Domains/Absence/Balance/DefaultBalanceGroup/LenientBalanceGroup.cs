namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient balance group is in its life cycle.</summary>
public enum LenientBalanceGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient balance group. Its code is unique within a tenant.</summary>
public sealed class LenientBalanceGroup(string code, string name, double hours, string description, double floorValue, int batchSize, string team, int retries, string country, string managerId, LenientBalanceGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public string Description { get; set; } = description;
    public double FloorValue { get; set; } = floorValue;
    public int BatchSize { get; set; } = batchSize;
    public string Team { get; set; } = team;
    public int Retries { get; set; } = retries;
    public string Country { get; set; } = country;
    public string ManagerId { get; set; } = managerId;
    public LenientBalanceGroupStatus Status { get; set; } = status;

    /// <summary>Starts a lenient balance group in status created.</summary>
    public static LenientBalanceGroup NewLenientBalanceGroup(string code, string name)
    {
        return new LenientBalanceGroup(
            code,
            name,
            0.0,
            "",
            0.0,
            0,
            "",
            0,
            "",
            "",
            LenientBalanceGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBalanceGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBalanceGroupStatus next)
    {
        switch (Status)
        {
            case LenientBalanceGroupStatus.Created:
                return next == LenientBalanceGroupStatus.Validated || next == LenientBalanceGroupStatus.Published;
            case LenientBalanceGroupStatus.Validated:
                return next == LenientBalanceGroupStatus.Published || next == LenientBalanceGroupStatus.Retired;
            case LenientBalanceGroupStatus.Published:
                switch (next)
                {
                    case LenientBalanceGroupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientBalanceGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient balance group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient balance group"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Retries + Hours, 2);
    }

    public LenientBalanceGroup Copy()
    {
        return new LenientBalanceGroup(
            Code,
            Name,
            Hours,
            Description,
            FloorValue,
            BatchSize,
            Team,
            Retries,
            Country,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
