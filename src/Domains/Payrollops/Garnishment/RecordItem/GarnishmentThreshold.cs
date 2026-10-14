namespace Acme.Workforce.Payrollops.Garnishment.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment threshold is in its life cycle.</summary>
public enum GarnishmentThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A garnishment threshold. Its code is unique within a tenant.</summary>
public sealed class GarnishmentThreshold(string code, string label, string contactEmail, string salaryBand, string costCenter, string category, double score, int batchSize, int endsAt, double cost, string currency, GarnishmentThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public string SalaryBand { get; set; } = salaryBand;
    public string CostCenter { get; set; } = costCenter;
    public string Category { get; set; } = category;
    public double Score { get; set; } = score;
    public int BatchSize { get; set; } = batchSize;
    public int EndsAt { get; set; } = endsAt;
    public double Cost { get; set; } = cost;
    public string Currency { get; set; } = currency;
    public GarnishmentThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment threshold in status draft.</summary>
    public static GarnishmentThreshold NewGarnishmentThreshold(string code, string label)
    {
        return new GarnishmentThreshold(
            code,
            label,
            "",
            "",
            "",
            "",
            0.0,
            0,
            0,
            0.0,
            "",
            GarnishmentThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentThresholdStatus next)
    {
        switch (Status)
        {
            case GarnishmentThresholdStatus.Draft:
                return next == GarnishmentThresholdStatus.Suspended;
            case GarnishmentThresholdStatus.Pending:
                return next == GarnishmentThresholdStatus.Suspended || next == GarnishmentThresholdStatus.Closed;
            case GarnishmentThresholdStatus.Active:
                return next == GarnishmentThresholdStatus.Closed;
            case GarnishmentThresholdStatus.Suspended:
                switch (next)
                {
                    case GarnishmentThresholdStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)EndsAt + Score, 2);
    }

    public GarnishmentThreshold Copy()
    {
        return new GarnishmentThreshold(
            Code,
            Label,
            ContactEmail,
            SalaryBand,
            CostCenter,
            Category,
            Score,
            BatchSize,
            EndsAt,
            Cost,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
