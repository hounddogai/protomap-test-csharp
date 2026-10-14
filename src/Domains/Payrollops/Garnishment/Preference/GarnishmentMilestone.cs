namespace Acme.Workforce.Payrollops.Garnishment.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment milestone is in its life cycle.</summary>
public enum GarnishmentMilestoneStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A garnishment milestone. Its code is unique within a tenant.</summary>
public sealed class GarnishmentMilestone(string code, string label, int leaveDays, double rate, string externalRef, string currency, GarnishmentMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rate { get; set; } = rate;
    public string ExternalRef { get; set; } = externalRef;
    public string Currency { get; set; } = currency;
    public GarnishmentMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment milestone in status open.</summary>
    public static GarnishmentMilestone NewGarnishmentMilestone(string code, string label)
    {
        return new GarnishmentMilestone(code, label, 0, 0.0, "", "", GarnishmentMilestoneStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentMilestoneStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentMilestoneStatus next)
    {
        switch (Status)
        {
            case GarnishmentMilestoneStatus.Open:
                return next == GarnishmentMilestoneStatus.Resolved;
            case GarnishmentMilestoneStatus.InReview:
                return next == GarnishmentMilestoneStatus.Resolved || next == GarnishmentMilestoneStatus.Archived;
            case GarnishmentMilestoneStatus.Resolved:
                switch (next)
                {
                    case GarnishmentMilestoneStatus.Archived:
                        return true;
                }
                return false;
            case GarnishmentMilestoneStatus.Reopened:
                switch (next)
                {
                    case GarnishmentMilestoneStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)LeaveDays - Rate, 2);
    }

    public GarnishmentMilestone Copy()
    {
        return new GarnishmentMilestone(Code, Label, LeaveDays, Rate, ExternalRef, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
