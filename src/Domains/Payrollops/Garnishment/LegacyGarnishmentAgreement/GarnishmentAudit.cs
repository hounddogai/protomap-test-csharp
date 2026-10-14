namespace Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment audit is in its life cycle.</summary>
public enum GarnishmentAuditStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a garnishment audit and its current status.</summary>
public sealed class GarnishmentAudit(string code, string name, string costCenter, int leaveDays, double minimum, int endsAt, int quantity, double margin, int expiresAt, string label, int revision, GarnishmentAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string CostCenter { get; set; } = costCenter;
    public int LeaveDays { get; set; } = leaveDays;
    public double Minimum { get; set; } = minimum;
    public int EndsAt { get; set; } = endsAt;
    public int Quantity { get; set; } = quantity;
    public double Margin { get; set; } = margin;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public GarnishmentAuditStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment audit in status open.</summary>
    public static GarnishmentAudit NewGarnishmentAudit(string code, string name)
    {
        return new GarnishmentAudit(code, name, "", 0, 0.0, 0, 0, 0.0, 0, "", 0, GarnishmentAuditStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentAuditStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentAuditStatus next)
    {
        switch (Status)
        {
            case GarnishmentAuditStatus.Open:
                switch (next)
                {
                    case GarnishmentAuditStatus.InReview:
                        return true;
                }
                return false;
            case GarnishmentAuditStatus.InReview:
                switch (next)
                {
                    case GarnishmentAuditStatus.Archived:
                        return true;
                }
                return false;
            case GarnishmentAuditStatus.Resolved:
                switch (next)
                {
                    case GarnishmentAuditStatus.Reopened:
                        return true;
                }
                return false;
            case GarnishmentAuditStatus.Reopened:
                switch (next)
                {
                    case GarnishmentAuditStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Quantity * (double)ExpiresAt, 2);
    }

    public GarnishmentAudit Copy()
    {
        return new GarnishmentAudit(
            Code,
            Name,
            CostCenter,
            LeaveDays,
            Minimum,
            EndsAt,
            Quantity,
            Margin,
            ExpiresAt,
            Label,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
