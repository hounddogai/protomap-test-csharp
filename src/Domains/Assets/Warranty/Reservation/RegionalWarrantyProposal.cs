namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional warranty proposal is in its life cycle.</summary>
public enum RegionalWarrantyProposalStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a regional warranty proposal and its current status.</summary>
public sealed class RegionalWarrantyProposal(string code, string name, string employeeId, string source, string category, int createdAt, string badge, double rating, RegionalWarrantyProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public string Source { get; set; } = source;
    public string Category { get; set; } = category;
    public int CreatedAt { get; set; } = createdAt;
    public string Badge { get; set; } = badge;
    public double Rating { get; set; } = rating;
    public RegionalWarrantyProposalStatus Status { get; set; } = status;

    /// <summary>Starts a regional warranty proposal in status created.</summary>
    public static RegionalWarrantyProposal NewRegionalWarrantyProposal(string code, string name)
    {
        return new RegionalWarrantyProposal(
            code,
            name,
            "",
            "",
            "",
            0,
            "",
            0.0,
            RegionalWarrantyProposalStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalWarrantyProposalStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalWarrantyProposalStatus next)
    {
        switch (Status)
        {
            case RegionalWarrantyProposalStatus.Created:
                switch (next)
                {
                    case RegionalWarrantyProposalStatus.Validated:
                        return true;
                }
                return false;
            case RegionalWarrantyProposalStatus.Validated:
                return next == RegionalWarrantyProposalStatus.Published || next == RegionalWarrantyProposalStatus.Retired;
            case RegionalWarrantyProposalStatus.Published:
                return next == RegionalWarrantyProposalStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RegionalWarrantyProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional warranty proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"regional warranty proposal"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)CreatedAt - Rating, 2);
    }

    public RegionalWarrantyProposal Copy()
    {
        return new RegionalWarrantyProposal(
            Code,
            Name,
            EmployeeId,
            Source,
            Category,
            CreatedAt,
            Badge,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
