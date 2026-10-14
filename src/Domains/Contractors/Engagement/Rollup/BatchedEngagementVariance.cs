namespace Acme.Workforce.Contractors.Engagement.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched engagement variance is in its life cycle.</summary>
public enum BatchedEngagementVarianceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A batched engagement variance. Its code is unique within a tenant.</summary>
public sealed class BatchedEngagementVariance(string code, string label, double amount, string skill, string country, BatchedEngagementVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public string Skill { get; set; } = skill;
    public string Country { get; set; } = country;
    public BatchedEngagementVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a batched engagement variance in status proposed.</summary>
    public static BatchedEngagementVariance NewBatchedEngagementVariance(string code, string label)
    {
        return new BatchedEngagementVariance(code, label, 0.0, "", "", BatchedEngagementVarianceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedEngagementVarianceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedEngagementVarianceStatus next)
    {
        switch (Status)
        {
            case BatchedEngagementVarianceStatus.Proposed:
                switch (next)
                {
                    case BatchedEngagementVarianceStatus.Approved:
                    case BatchedEngagementVarianceStatus.Rejected:
                        return true;
                }
                return false;
            case BatchedEngagementVarianceStatus.Approved:
                switch (next)
                {
                    case BatchedEngagementVarianceStatus.Withdrawn:
                        return true;
                }
                return false;
            case BatchedEngagementVarianceStatus.Rejected:
                return next == BatchedEngagementVarianceStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BatchedEngagementVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched engagement variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public BatchedEngagementVariance Copy()
    {
        return new BatchedEngagementVariance(Code, Label, Amount, Skill, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
