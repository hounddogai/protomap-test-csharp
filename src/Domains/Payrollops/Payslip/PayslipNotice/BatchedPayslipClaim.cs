namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched payslip claim is in its life cycle.</summary>
public enum BatchedPayslipClaimStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a batched payslip claim and its current status.</summary>
public sealed class BatchedPayslipClaim(string code, string name, int capacity, string externalRef, string segment, BatchedPayslipClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public string ExternalRef { get; set; } = externalRef;
    public string Segment { get; set; } = segment;
    public BatchedPayslipClaimStatus Status { get; set; } = status;

    /// <summary>Starts a batched payslip claim in status requested.</summary>
    public static BatchedPayslipClaim NewBatchedPayslipClaim(string code, string name)
    {
        return new BatchedPayslipClaim(code, name, 0, "", "", BatchedPayslipClaimStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPayslipClaimStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPayslipClaimStatus next)
    {
        switch (Status)
        {
            case BatchedPayslipClaimStatus.Requested:
                switch (next)
                {
                    case BatchedPayslipClaimStatus.Granted:
                    case BatchedPayslipClaimStatus.Expired:
                        return true;
                }
                return false;
            case BatchedPayslipClaimStatus.Granted:
                switch (next)
                {
                    case BatchedPayslipClaimStatus.Revoked:
                    case BatchedPayslipClaimStatus.Expired:
                        return true;
                }
                return false;
            case BatchedPayslipClaimStatus.Revoked:
                return next == BatchedPayslipClaimStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BatchedPayslipClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched payslip claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Capacity;
    }

    public BatchedPayslipClaim Copy()
    {
        return new BatchedPayslipClaim(Code, Name, Capacity, ExternalRef, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
