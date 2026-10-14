namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span proposal is in its life cycle.</summary>
public enum SpanProposalStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A span proposal. Its code is unique within a tenant.</summary>
public sealed class SpanProposal(string code, string title, string salaryBand, string checksum, string phone, int createdAt, string name, string segment, double score, double budget, double amount, SpanProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public string Checksum { get; set; } = checksum;
    public string Phone { get; set; } = phone;
    public int CreatedAt { get; set; } = createdAt;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public double Score { get; set; } = score;
    public double Budget { get; set; } = budget;
    public double Amount { get; set; } = amount;
    public SpanProposalStatus Status { get; set; } = status;

    /// <summary>Starts a span proposal in status proposed.</summary>
    public static SpanProposal NewSpanProposal(string code, string title)
    {
        return new SpanProposal(code, title, "", "", "", 0, "", "", 0.0, 0.0, 0.0, SpanProposalStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanProposalStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanProposalStatus next)
    {
        switch (Status)
        {
            case SpanProposalStatus.Proposed:
                return next == SpanProposalStatus.Approved;
            case SpanProposalStatus.Approved:
                switch (next)
                {
                    case SpanProposalStatus.Withdrawn:
                        return true;
                }
                return false;
            case SpanProposalStatus.Rejected:
                switch (next)
                {
                    case SpanProposalStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"span proposal"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)CreatedAt + Budget, 2);
    }

    public SpanProposal Copy()
    {
        return new SpanProposal(
            Code,
            Title,
            SalaryBand,
            Checksum,
            Phone,
            CreatedAt,
            Name,
            Segment,
            Score,
            Budget,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
