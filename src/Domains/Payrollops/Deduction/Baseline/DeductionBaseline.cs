namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction baseline is in its life cycle.</summary>
public enum DeductionBaselineStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A deduction baseline. Its code is unique within a tenant.</summary>
public sealed class DeductionBaseline(string code, string title, bool archived, int batchSize, int startsAt, DeductionBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public int BatchSize { get; set; } = batchSize;
    public int StartsAt { get; set; } = startsAt;
    public DeductionBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a deduction baseline in status created.</summary>
    public static DeductionBaseline NewDeductionBaseline(string code, string title)
    {
        return new DeductionBaseline(code, title, false, 0, 0, DeductionBaselineStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionBaselineStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionBaselineStatus next)
    {
        switch (Status)
        {
            case DeductionBaselineStatus.Created:
                return next == DeductionBaselineStatus.Validated;
            case DeductionBaselineStatus.Validated:
                switch (next)
                {
                    case DeductionBaselineStatus.Published:
                    case DeductionBaselineStatus.Retired:
                        return true;
                }
                return false;
            case DeductionBaselineStatus.Published:
                return next == DeductionBaselineStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DeductionBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)StartsAt - (double)BatchSize, 2);
    }

    public DeductionBaseline Copy()
    {
        return new DeductionBaseline(Code, Title, Archived, BatchSize, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
