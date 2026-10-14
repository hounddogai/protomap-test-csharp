namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty proposal is in its life cycle.</summary>
public enum WarrantyProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a warranty proposal and its current status.</summary>
public sealed class WarrantyProposal(string code, string title, Dictionary<string, string> attributes, bool active, string badge, double discount, string category, double ratio, bool visible, string ownerId, WarrantyProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Active { get; set; } = active;
    public string Badge { get; set; } = badge;
    public double Discount { get; set; } = discount;
    public string Category { get; set; } = category;
    public double Ratio { get; set; } = ratio;
    public bool Visible { get; set; } = visible;
    public string OwnerId { get; set; } = ownerId;
    public WarrantyProposalStatus Status { get; set; } = status;

    /// <summary>Starts a warranty proposal in status planned.</summary>
    public static WarrantyProposal NewWarrantyProposal(string code, string title)
    {
        return new WarrantyProposal(
            code,
            title,
            new Dictionary<string, string>(),
            false,
            "",
            0.0,
            "",
            0.0,
            false,
            "",
            WarrantyProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyProposalStatus next)
    {
        switch (Status)
        {
            case WarrantyProposalStatus.Planned:
                switch (next)
                {
                    case WarrantyProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case WarrantyProposalStatus.Scheduled:
                return next == WarrantyProposalStatus.InProgress || next == WarrantyProposalStatus.Completed;
            case WarrantyProposalStatus.InProgress:
                return next == WarrantyProposalStatus.Cancelled;
            case WarrantyProposalStatus.Completed:
                switch (next)
                {
                    case WarrantyProposalStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedLoad()
    {
        return Math.Round(Discount - Ratio, 2);
    }

    public WarrantyProposal Copy()
    {
        return new WarrantyProposal(
            Code,
            Title,
            Attributes,
            Active,
            Badge,
            Discount,
            Category,
            Ratio,
            Visible,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
