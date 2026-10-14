namespace Acme.Workforce.Compliance.Certification.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification summary is in its life cycle.</summary>
public enum CertificationSummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One certification summary, with what the planners need to act on it.</summary>
public sealed class CertificationSummary(string code, string label, double maximum, double cost, double margin, string source, string costCenter, bool approved, string badge, CertificationSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public double Cost { get; set; } = cost;
    public double Margin { get; set; } = margin;
    public string Source { get; set; } = source;
    public string CostCenter { get; set; } = costCenter;
    public bool Approved { get; set; } = approved;
    public string Badge { get; set; } = badge;
    public CertificationSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a certification summary in status proposed.</summary>
    public static CertificationSummary NewCertificationSummary(string code, string label)
    {
        return new CertificationSummary(
            code,
            label,
            0.0,
            0.0,
            0.0,
            "",
            "",
            false,
            "",
            CertificationSummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationSummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationSummaryStatus next)
    {
        switch (Status)
        {
            case CertificationSummaryStatus.Proposed:
                return next == CertificationSummaryStatus.Approved || next == CertificationSummaryStatus.Withdrawn;
            case CertificationSummaryStatus.Approved:
                return next == CertificationSummaryStatus.Rejected || next == CertificationSummaryStatus.Withdrawn;
            case CertificationSummaryStatus.Rejected:
                switch (next)
                {
                    case CertificationSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Margin - Cost, 2);
    }

    public CertificationSummary Copy()
    {
        return new CertificationSummary(
            Code,
            Label,
            Maximum,
            Cost,
            Margin,
            Source,
            CostCenter,
            Approved,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
