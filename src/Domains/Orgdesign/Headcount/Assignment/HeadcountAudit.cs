namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount audit is in its life cycle.</summary>
public enum HeadcountAuditStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A headcount audit. Its code is unique within a tenant.</summary>
public sealed class HeadcountAudit(string code, string label, string costCenter, string badge, double amount, string channel, double score, string managerId, string country, string ownerId, string title, HeadcountAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public string Badge { get; set; } = badge;
    public double Amount { get; set; } = amount;
    public string Channel { get; set; } = channel;
    public double Score { get; set; } = score;
    public string ManagerId { get; set; } = managerId;
    public string Country { get; set; } = country;
    public string OwnerId { get; set; } = ownerId;
    public string Title { get; set; } = title;
    public HeadcountAuditStatus Status { get; set; } = status;

    /// <summary>Starts a headcount audit in status open.</summary>
    public static HeadcountAudit NewHeadcountAudit(string code, string label)
    {
        return new HeadcountAudit(code, label, "", "", 0.0, "", 0.0, "", "", "", "", HeadcountAuditStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountAuditStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountAuditStatus next)
    {
        switch (Status)
        {
            case HeadcountAuditStatus.Open:
                switch (next)
                {
                    case HeadcountAuditStatus.InReview:
                    case HeadcountAuditStatus.Archived:
                        return true;
                }
                return false;
            case HeadcountAuditStatus.InReview:
                return next == HeadcountAuditStatus.Reopened || next == HeadcountAuditStatus.Archived;
            case HeadcountAuditStatus.Resolved:
                return next == HeadcountAuditStatus.Reopened;
            case HeadcountAuditStatus.Reopened:
                switch (next)
                {
                    case HeadcountAuditStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Amount - Score, 2);
    }

    public HeadcountAudit Copy()
    {
        return new HeadcountAudit(
            Code,
            Label,
            CostCenter,
            Badge,
            Amount,
            Channel,
            Score,
            ManagerId,
            Country,
            OwnerId,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
