namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction batch is in its life cycle.</summary>
public enum DeductionBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a deduction batch and its current status.</summary>
public sealed class DeductionBatch(string code, string label, List<string> tags, double rate, int expiresAt, DeductionBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public double Rate { get; set; } = rate;
    public int ExpiresAt { get; set; } = expiresAt;
    public DeductionBatchStatus Status { get; set; } = status;

    /// <summary>Starts a deduction batch in status open.</summary>
    public static DeductionBatch NewDeductionBatch(string code, string label)
    {
        return new DeductionBatch(code, label, new List<string>(), 0.0, 0, DeductionBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionBatchStatus next)
    {
        switch (Status)
        {
            case DeductionBatchStatus.Open:
                switch (next)
                {
                    case DeductionBatchStatus.InReview:
                        return true;
                }
                return false;
            case DeductionBatchStatus.InReview:
                return next == DeductionBatchStatus.Resolved || next == DeductionBatchStatus.Reopened;
            case DeductionBatchStatus.Resolved:
                switch (next)
                {
                    case DeductionBatchStatus.Reopened:
                    case DeductionBatchStatus.Archived:
                        return true;
                }
                return false;
            case DeductionBatchStatus.Reopened:
                switch (next)
                {
                    case DeductionBatchStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double NetVolume()
    {
        return Math.Round((double)ExpiresAt * Rate, 2);
    }

    public DeductionBatch Copy()
    {
        return new DeductionBatch(Code, Label, Tags, Rate, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
