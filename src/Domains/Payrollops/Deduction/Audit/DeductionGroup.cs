namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction group is in its life cycle.</summary>
public enum DeductionGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A deduction group. Its code is unique within a tenant.</summary>
public sealed class DeductionGroup(string code, string title, string badge, int batchSize, double discount, string employeeId, string reviewer, string channel, string contactEmail, string notes, bool visible, DeductionGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public int BatchSize { get; set; } = batchSize;
    public double Discount { get; set; } = discount;
    public string EmployeeId { get; set; } = employeeId;
    public string Reviewer { get; set; } = reviewer;
    public string Channel { get; set; } = channel;
    public string ContactEmail { get; set; } = contactEmail;
    public string Notes { get; set; } = notes;
    public bool Visible { get; set; } = visible;
    public DeductionGroupStatus Status { get; set; } = status;

    /// <summary>Starts a deduction group in status created.</summary>
    public static DeductionGroup NewDeductionGroup(string code, string title)
    {
        return new DeductionGroup(code, title, "", 0, 0.0, "", "", "", "", "", false, DeductionGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionGroupStatus next)
    {
        switch (Status)
        {
            case DeductionGroupStatus.Created:
                return next == DeductionGroupStatus.Published;
            case DeductionGroupStatus.Validated:
                return next == DeductionGroupStatus.Published || next == DeductionGroupStatus.Retired;
            case DeductionGroupStatus.Published:
                switch (next)
                {
                    case DeductionGroupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)BatchSize * Discount, 2);
    }

    public DeductionGroup Copy()
    {
        return new DeductionGroup(
            Code,
            Title,
            Badge,
            BatchSize,
            Discount,
            EmployeeId,
            Reviewer,
            Channel,
            ContactEmail,
            Notes,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
