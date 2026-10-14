namespace Acme.Workforce.Payrollops.Payslip.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip claim is in its life cycle.</summary>
public enum PayslipClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One payslip claim, with what the planners need to act on it.</summary>
public sealed class PayslipClaim(string code, string label, string summary, string category, string source, string timezone, string managerId, int limit, string segment, double margin, PayslipClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public string Category { get; set; } = category;
    public string Source { get; set; } = source;
    public string Timezone { get; set; } = timezone;
    public string ManagerId { get; set; } = managerId;
    public int Limit { get; set; } = limit;
    public string Segment { get; set; } = segment;
    public double Margin { get; set; } = margin;
    public PayslipClaimStatus Status { get; set; } = status;

    /// <summary>Starts a payslip claim in status draft.</summary>
    public static PayslipClaim NewPayslipClaim(string code, string label)
    {
        return new PayslipClaim(code, label, "", "", "", "", "", 0, "", 0.0, PayslipClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipClaimStatus next)
    {
        switch (Status)
        {
            case PayslipClaimStatus.Draft:
                return next == PayslipClaimStatus.Suspended;
            case PayslipClaimStatus.Pending:
                switch (next)
                {
                    case PayslipClaimStatus.Active:
                        return true;
                }
                return false;
            case PayslipClaimStatus.Active:
                switch (next)
                {
                    case PayslipClaimStatus.Suspended:
                    case PayslipClaimStatus.Closed:
                        return true;
                }
                return false;
            case PayslipClaimStatus.Suspended:
                return next == PayslipClaimStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PayslipClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"payslip claim"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Limit - Margin, 2);
    }

    public PayslipClaim Copy()
    {
        return new PayslipClaim(
            Code,
            Label,
            Summary,
            Category,
            Source,
            Timezone,
            ManagerId,
            Limit,
            Segment,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
