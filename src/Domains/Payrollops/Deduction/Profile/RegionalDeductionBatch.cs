namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional deduction batch is in its life cycle.</summary>
public enum RegionalDeductionBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a regional deduction batch and its current status.</summary>
public sealed class RegionalDeductionBatch(string code, string name, double maximum, int endsAt, int tenureMonths, string source, double cost, string skill, string ownerId, bool archived, int version, RegionalDeductionBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public int EndsAt { get; set; } = endsAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Source { get; set; } = source;
    public double Cost { get; set; } = cost;
    public string Skill { get; set; } = skill;
    public string OwnerId { get; set; } = ownerId;
    public bool Archived { get; set; } = archived;
    public int Version { get; set; } = version;
    public RegionalDeductionBatchStatus Status { get; set; } = status;

    /// <summary>Starts a regional deduction batch in status created.</summary>
    public static RegionalDeductionBatch NewRegionalDeductionBatch(string code, string name)
    {
        return new RegionalDeductionBatch(
            code,
            name,
            0.0,
            0,
            0,
            "",
            0.0,
            "",
            "",
            false,
            0,
            RegionalDeductionBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalDeductionBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalDeductionBatchStatus next)
    {
        switch (Status)
        {
            case RegionalDeductionBatchStatus.Created:
                switch (next)
                {
                    case RegionalDeductionBatchStatus.Validated:
                    case RegionalDeductionBatchStatus.Published:
                        return true;
                }
                return false;
            case RegionalDeductionBatchStatus.Validated:
                return next == RegionalDeductionBatchStatus.Published || next == RegionalDeductionBatchStatus.Retired;
            case RegionalDeductionBatchStatus.Published:
                switch (next)
                {
                    case RegionalDeductionBatchStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalDeductionBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional deduction batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)TenureMonths * Maximum, 2);
    }

    public RegionalDeductionBatch Copy()
    {
        return new RegionalDeductionBatch(
            Code,
            Name,
            Maximum,
            EndsAt,
            TenureMonths,
            Source,
            Cost,
            Skill,
            OwnerId,
            Archived,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
