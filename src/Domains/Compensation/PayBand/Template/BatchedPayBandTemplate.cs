namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched pay band template is in its life cycle.</summary>
public enum BatchedPayBandTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A batched pay band template as the pay band team keeps it.</summary>
public sealed class BatchedPayBandTemplate(string code, string label, string summary, string salaryBand, double discount, string unit, string source, int updatedAt, bool archived, string location, BatchedPayBandTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public string SalaryBand { get; set; } = salaryBand;
    public double Discount { get; set; } = discount;
    public string Unit { get; set; } = unit;
    public string Source { get; set; } = source;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Archived { get; set; } = archived;
    public string Location { get; set; } = location;
    public BatchedPayBandTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a batched pay band template in status open.</summary>
    public static BatchedPayBandTemplate NewBatchedPayBandTemplate(string code, string label)
    {
        return new BatchedPayBandTemplate(
            code,
            label,
            "",
            "",
            0.0,
            "",
            "",
            0,
            false,
            "",
            BatchedPayBandTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPayBandTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPayBandTemplateStatus next)
    {
        switch (Status)
        {
            case BatchedPayBandTemplateStatus.Open:
                return next == BatchedPayBandTemplateStatus.InReview || next == BatchedPayBandTemplateStatus.Reopened;
            case BatchedPayBandTemplateStatus.InReview:
                switch (next)
                {
                    case BatchedPayBandTemplateStatus.Resolved:
                        return true;
                }
                return false;
            case BatchedPayBandTemplateStatus.Resolved:
                switch (next)
                {
                    case BatchedPayBandTemplateStatus.Reopened:
                    case BatchedPayBandTemplateStatus.Archived:
                        return true;
                }
                return false;
            case BatchedPayBandTemplateStatus.Reopened:
                return next == BatchedPayBandTemplateStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BatchedPayBandTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched pay band template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)UpdatedAt - Discount, 2);
    }

    public BatchedPayBandTemplate Copy()
    {
        return new BatchedPayBandTemplate(
            Code,
            Label,
            Summary,
            SalaryBand,
            Discount,
            Unit,
            Source,
            UpdatedAt,
            Archived,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
