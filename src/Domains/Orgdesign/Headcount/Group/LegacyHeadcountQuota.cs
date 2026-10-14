namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy headcount quota is in its life cycle.</summary>
public enum LegacyHeadcountQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a legacy headcount quota and its current status.</summary>
public sealed class LegacyHeadcountQuota(string code, string label, string department, string jobCode, string region, double amount, LegacyHeadcountQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public string JobCode { get; set; } = jobCode;
    public string Region { get; set; } = region;
    public double Amount { get; set; } = amount;
    public LegacyHeadcountQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a legacy headcount quota in status open.</summary>
    public static LegacyHeadcountQuota NewLegacyHeadcountQuota(string code, string label)
    {
        return new LegacyHeadcountQuota(code, label, "", "", "", 0.0, LegacyHeadcountQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyHeadcountQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyHeadcountQuotaStatus next)
    {
        switch (Status)
        {
            case LegacyHeadcountQuotaStatus.Open:
                return next == LegacyHeadcountQuotaStatus.InReview || next == LegacyHeadcountQuotaStatus.Archived;
            case LegacyHeadcountQuotaStatus.InReview:
                return next == LegacyHeadcountQuotaStatus.Resolved;
            case LegacyHeadcountQuotaStatus.Resolved:
                switch (next)
                {
                    case LegacyHeadcountQuotaStatus.Reopened:
                        return true;
                }
                return false;
            case LegacyHeadcountQuotaStatus.Reopened:
                return next == LegacyHeadcountQuotaStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LegacyHeadcountQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy headcount quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public LegacyHeadcountQuota Copy()
    {
        return new LegacyHeadcountQuota(Code, Label, Department, JobCode, Region, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
