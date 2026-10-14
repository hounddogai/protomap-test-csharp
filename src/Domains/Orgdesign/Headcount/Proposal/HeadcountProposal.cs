namespace Acme.Workforce.Orgdesign.Headcount.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount proposal is in its life cycle.</summary>
public enum HeadcountProposalStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A headcount proposal. Its code is unique within a tenant.</summary>
public sealed class HeadcountProposal(string code, string title, int updatedAt, int tenureMonths, int expiresAt, string skill, double floorValue, double weight, HeadcountProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Skill { get; set; } = skill;
    public double FloorValue { get; set; } = floorValue;
    public double Weight { get; set; } = weight;
    public HeadcountProposalStatus Status { get; set; } = status;

    /// <summary>Starts a headcount proposal in status requested.</summary>
    public static HeadcountProposal NewHeadcountProposal(string code, string title)
    {
        return new HeadcountProposal(code, title, 0, 0, 0, "", 0.0, 0.0, HeadcountProposalStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountProposalStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountProposalStatus next)
    {
        switch (Status)
        {
            case HeadcountProposalStatus.Requested:
                switch (next)
                {
                    case HeadcountProposalStatus.Revoked:
                        return true;
                }
                return false;
            case HeadcountProposalStatus.Granted:
                switch (next)
                {
                    case HeadcountProposalStatus.Expired:
                        return true;
                }
                return false;
            case HeadcountProposalStatus.Revoked:
                switch (next)
                {
                    case HeadcountProposalStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetVolume()
    {
        return Math.Round(Weight * (double)ExpiresAt, 2);
    }

    public HeadcountProposal Copy()
    {
        return new HeadcountProposal(
            Code,
            Title,
            UpdatedAt,
            TenureMonths,
            ExpiresAt,
            Skill,
            FloorValue,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
