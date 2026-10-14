namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled succession plan audit is in its life cycle.</summary>
public enum ScheduledSuccessionPlanAuditStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One scheduled succession plan audit, with what the planners need to act on it.</summary>
public sealed class ScheduledSuccessionPlanAudit(string code, string title, string location, int attempts, string costCenter, double discount, string country, string reviewer, string source, ScheduledSuccessionPlanAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public int Attempts { get; set; } = attempts;
    public string CostCenter { get; set; } = costCenter;
    public double Discount { get; set; } = discount;
    public string Country { get; set; } = country;
    public string Reviewer { get; set; } = reviewer;
    public string Source { get; set; } = source;
    public ScheduledSuccessionPlanAuditStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled succession plan audit in status proposed.</summary>
    public static ScheduledSuccessionPlanAudit NewScheduledSuccessionPlanAudit(string code, string title)
    {
        return new ScheduledSuccessionPlanAudit(
            code,
            title,
            "",
            0,
            "",
            0.0,
            "",
            "",
            "",
            ScheduledSuccessionPlanAuditStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledSuccessionPlanAuditStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledSuccessionPlanAuditStatus next)
    {
        switch (Status)
        {
            case ScheduledSuccessionPlanAuditStatus.Proposed:
                switch (next)
                {
                    case ScheduledSuccessionPlanAuditStatus.Rejected:
                        return true;
                }
                return false;
            case ScheduledSuccessionPlanAuditStatus.Approved:
                return next == ScheduledSuccessionPlanAuditStatus.Rejected || next == ScheduledSuccessionPlanAuditStatus.Withdrawn;
            case ScheduledSuccessionPlanAuditStatus.Rejected:
                switch (next)
                {
                    case ScheduledSuccessionPlanAuditStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledSuccessionPlanAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled succession plan audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Attempts - Discount, 2);
    }

    public ScheduledSuccessionPlanAudit Copy()
    {
        return new ScheduledSuccessionPlanAudit(
            Code,
            Title,
            Location,
            Attempts,
            CostCenter,
            Discount,
            Country,
            Reviewer,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
