namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary bonus pool tier is in its life cycle.</summary>
public enum PrimaryBonusPoolTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a primary bonus pool tier and its current status.</summary>
public sealed class PrimaryBonusPoolTier(string code, string title, string costCenter, string salaryBand, string employeeId, List<string> tags, string unit, int limit, double maximum, string currency, PrimaryBonusPoolTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public string SalaryBand { get; set; } = salaryBand;
    public string EmployeeId { get; set; } = employeeId;
    public List<string> Tags { get; set; } = tags;
    public string Unit { get; set; } = unit;
    public int Limit { get; set; } = limit;
    public double Maximum { get; set; } = maximum;
    public string Currency { get; set; } = currency;
    public PrimaryBonusPoolTierStatus Status { get; set; } = status;

    /// <summary>Starts a primary bonus pool tier in status created.</summary>
    public static PrimaryBonusPoolTier NewPrimaryBonusPoolTier(string code, string title)
    {
        return new PrimaryBonusPoolTier(
            code,
            title,
            "",
            "",
            "",
            new List<string>(),
            "",
            0,
            0.0,
            "",
            PrimaryBonusPoolTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryBonusPoolTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryBonusPoolTierStatus next)
    {
        switch (Status)
        {
            case PrimaryBonusPoolTierStatus.Created:
                return next == PrimaryBonusPoolTierStatus.Published;
            case PrimaryBonusPoolTierStatus.Validated:
                return next == PrimaryBonusPoolTierStatus.Retired;
            case PrimaryBonusPoolTierStatus.Published:
                switch (next)
                {
                    case PrimaryBonusPoolTierStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryBonusPoolTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary bonus pool tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedLoad()
    {
        return Math.Round(Maximum * (double)Limit, 2);
    }

    public PrimaryBonusPoolTier Copy()
    {
        return new PrimaryBonusPoolTier(
            Code,
            Title,
            CostCenter,
            SalaryBand,
            EmployeeId,
            Tags,
            Unit,
            Limit,
            Maximum,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
