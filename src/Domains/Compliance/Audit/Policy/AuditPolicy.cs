namespace Acme.Workforce.Compliance.Audit.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit policy is in its life cycle.</summary>
public enum AuditPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a audit policy and its current status.</summary>
public sealed class AuditPolicy(string code, string name, string segment, double maximum, string label, string region, double rate, string locale, double margin, int tenureMonths, string country, AuditPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public double Maximum { get; set; } = maximum;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public double Rate { get; set; } = rate;
    public string Locale { get; set; } = locale;
    public double Margin { get; set; } = margin;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Country { get; set; } = country;
    public AuditPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a audit policy in status new.</summary>
    public static AuditPolicy NewAuditPolicy(string code, string name)
    {
        return new AuditPolicy(code, name, "", 0.0, "", "", 0.0, "", 0.0, 0, "", AuditPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditPolicyStatus next)
    {
        switch (Status)
        {
            case AuditPolicyStatus.New:
                return next == AuditPolicyStatus.Queued;
            case AuditPolicyStatus.Queued:
                return next == AuditPolicyStatus.Running || next == AuditPolicyStatus.Failed;
            case AuditPolicyStatus.Running:
                return next == AuditPolicyStatus.Done || next == AuditPolicyStatus.Failed;
            case AuditPolicyStatus.Done:
                return next == AuditPolicyStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AuditPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double NetTotal()
    {
        return Math.Round(Margin + Rate, 2);
    }

    public AuditPolicy Copy()
    {
        return new AuditPolicy(
            Code,
            Name,
            Segment,
            Maximum,
            Label,
            Region,
            Rate,
            Locale,
            Margin,
            TenureMonths,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
