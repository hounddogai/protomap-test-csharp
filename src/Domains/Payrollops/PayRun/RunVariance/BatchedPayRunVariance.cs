namespace Acme.Workforce.Payrollops.PayRun.RunVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched pay run variance is in its life cycle.</summary>
public enum BatchedPayRunVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A batched pay run variance. Its code is unique within a tenant.</summary>
public sealed class BatchedPayRunVariance(string code, string title, double discount, int expiresAt, string team, string summary, string skill, double floorValue, double minimum, string salaryBand, string costCenter, BatchedPayRunVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Team { get; set; } = team;
    public string Summary { get; set; } = summary;
    public string Skill { get; set; } = skill;
    public double FloorValue { get; set; } = floorValue;
    public double Minimum { get; set; } = minimum;
    public string SalaryBand { get; set; } = salaryBand;
    public string CostCenter { get; set; } = costCenter;
    public BatchedPayRunVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a batched pay run variance in status created.</summary>
    public static BatchedPayRunVariance NewBatchedPayRunVariance(string code, string title)
    {
        return new BatchedPayRunVariance(
            code,
            title,
            0.0,
            0,
            "",
            "",
            "",
            0.0,
            0.0,
            "",
            "",
            BatchedPayRunVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPayRunVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPayRunVarianceStatus next)
    {
        switch (Status)
        {
            case BatchedPayRunVarianceStatus.Created:
                switch (next)
                {
                    case BatchedPayRunVarianceStatus.Published:
                    case BatchedPayRunVarianceStatus.Retired:
                        return true;
                }
                return false;
            case BatchedPayRunVarianceStatus.Validated:
                return next == BatchedPayRunVarianceStatus.Published || next == BatchedPayRunVarianceStatus.Retired;
            case BatchedPayRunVarianceStatus.Published:
                switch (next)
                {
                    case BatchedPayRunVarianceStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedPayRunVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched pay run variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)ExpiresAt * FloorValue, 2);
    }

    public BatchedPayRunVariance Copy()
    {
        return new BatchedPayRunVariance(
            Code,
            Title,
            Discount,
            ExpiresAt,
            Team,
            Summary,
            Skill,
            FloorValue,
            Minimum,
            SalaryBand,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
