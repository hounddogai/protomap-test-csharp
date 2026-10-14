namespace Acme.Workforce.Assets.AssetReturn.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return proposal is in its life cycle.</summary>
public enum AssetReturnProposalStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A asset return proposal. Its code is unique within a tenant.</summary>
public sealed class AssetReturnProposal(string code, string label, int quantity, bool approved, int startsAt, AssetReturnProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public bool Approved { get; set; } = approved;
    public int StartsAt { get; set; } = startsAt;
    public AssetReturnProposalStatus Status { get; set; } = status;

    /// <summary>Starts a asset return proposal in status proposed.</summary>
    public static AssetReturnProposal NewAssetReturnProposal(string code, string label)
    {
        return new AssetReturnProposal(code, label, 0, false, 0, AssetReturnProposalStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnProposalStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnProposalStatus next)
    {
        switch (Status)
        {
            case AssetReturnProposalStatus.Proposed:
                switch (next)
                {
                    case AssetReturnProposalStatus.Approved:
                    case AssetReturnProposalStatus.Rejected:
                        return true;
                }
                return false;
            case AssetReturnProposalStatus.Approved:
                return next == AssetReturnProposalStatus.Withdrawn;
            case AssetReturnProposalStatus.Rejected:
                return next == AssetReturnProposalStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AssetReturnProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Quantity + (double)StartsAt, 2);
    }

    public AssetReturnProposal Copy()
    {
        return new AssetReturnProposal(Code, Label, Quantity, Approved, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
