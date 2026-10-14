namespace Acme.Workforce.Payrollops.Payslip.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched payslip proposal is in its life cycle.</summary>
public enum BatchedPayslipProposalStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A batched payslip proposal as the payslip team keeps it.</summary>
public sealed class BatchedPayslipProposal(string code, string name, int batchSize, double weight, string description, string segment, string contactEmail, double ceiling, BatchedPayslipProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public double Weight { get; set; } = weight;
    public string Description { get; set; } = description;
    public string Segment { get; set; } = segment;
    public string ContactEmail { get; set; } = contactEmail;
    public double Ceiling { get; set; } = ceiling;
    public BatchedPayslipProposalStatus Status { get; set; } = status;

    /// <summary>Starts a batched payslip proposal in status created.</summary>
    public static BatchedPayslipProposal NewBatchedPayslipProposal(string code, string name)
    {
        return new BatchedPayslipProposal(code, name, 0, 0.0, "", "", "", 0.0, BatchedPayslipProposalStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPayslipProposalStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPayslipProposalStatus next)
    {
        switch (Status)
        {
            case BatchedPayslipProposalStatus.Created:
                switch (next)
                {
                    case BatchedPayslipProposalStatus.Retired:
                        return true;
                }
                return false;
            case BatchedPayslipProposalStatus.Validated:
                switch (next)
                {
                    case BatchedPayslipProposalStatus.Published:
                    case BatchedPayslipProposalStatus.Retired:
                        return true;
                }
                return false;
            case BatchedPayslipProposalStatus.Published:
                return next == BatchedPayslipProposalStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BatchedPayslipProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched payslip proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)BatchSize - Weight, 2);
    }

    public BatchedPayslipProposal Copy()
    {
        return new BatchedPayslipProposal(
            Code,
            Name,
            BatchSize,
            Weight,
            Description,
            Segment,
            ContactEmail,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
