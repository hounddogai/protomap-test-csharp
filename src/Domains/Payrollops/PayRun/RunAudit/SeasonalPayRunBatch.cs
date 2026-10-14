namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal pay run batch is in its life cycle.</summary>
public enum SeasonalPayRunBatchStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a seasonal pay run batch and its current status.</summary>
public sealed class SeasonalPayRunBatch(string code, string label, string grade, string contactEmail, int tenureMonths, string region, int quantity, int graceDays, double discount, string skill, string segment, SeasonalPayRunBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public string ContactEmail { get; set; } = contactEmail;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Region { get; set; } = region;
    public int Quantity { get; set; } = quantity;
    public int GraceDays { get; set; } = graceDays;
    public double Discount { get; set; } = discount;
    public string Skill { get; set; } = skill;
    public string Segment { get; set; } = segment;
    public SeasonalPayRunBatchStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal pay run batch in status proposed.</summary>
    public static SeasonalPayRunBatch NewSeasonalPayRunBatch(string code, string label)
    {
        return new SeasonalPayRunBatch(
            code,
            label,
            "",
            "",
            0,
            "",
            0,
            0,
            0.0,
            "",
            "",
            SeasonalPayRunBatchStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPayRunBatchStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPayRunBatchStatus next)
    {
        switch (Status)
        {
            case SeasonalPayRunBatchStatus.Proposed:
                return next == SeasonalPayRunBatchStatus.Rejected || next == SeasonalPayRunBatchStatus.Withdrawn;
            case SeasonalPayRunBatchStatus.Approved:
                switch (next)
                {
                    case SeasonalPayRunBatchStatus.Rejected:
                    case SeasonalPayRunBatchStatus.Withdrawn:
                        return true;
                }
                return false;
            case SeasonalPayRunBatchStatus.Rejected:
                return next == SeasonalPayRunBatchStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SeasonalPayRunBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal pay run batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)TenureMonths + (double)GraceDays, 2);
    }

    public SeasonalPayRunBatch Copy()
    {
        return new SeasonalPayRunBatch(
            Code,
            Label,
            Grade,
            ContactEmail,
            TenureMonths,
            Region,
            Quantity,
            GraceDays,
            Discount,
            Skill,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
