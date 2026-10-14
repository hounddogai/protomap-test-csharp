namespace Acme.Workforce.Orgdesign.CostModel.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model claim is in its life cycle.</summary>
public enum CostModelClaimStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a cost model claim and its current status.</summary>
public sealed class CostModelClaim(string code, string name, int startsAt, double ratio, int tenureMonths, string title, string country, string team, double cost, string managerId, string notes, CostModelClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public double Ratio { get; set; } = ratio;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public string Team { get; set; } = team;
    public double Cost { get; set; } = cost;
    public string ManagerId { get; set; } = managerId;
    public string Notes { get; set; } = notes;
    public CostModelClaimStatus Status { get; set; } = status;

    /// <summary>Starts a cost model claim in status requested.</summary>
    public static CostModelClaim NewCostModelClaim(string code, string name)
    {
        return new CostModelClaim(code, name, 0, 0.0, 0, "", "", "", 0.0, "", "", CostModelClaimStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelClaimStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelClaimStatus next)
    {
        switch (Status)
        {
            case CostModelClaimStatus.Requested:
                return next == CostModelClaimStatus.Expired;
            case CostModelClaimStatus.Granted:
                return next == CostModelClaimStatus.Revoked || next == CostModelClaimStatus.Expired;
            case CostModelClaimStatus.Revoked:
                switch (next)
                {
                    case CostModelClaimStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Cost - Ratio, 2);
    }

    public CostModelClaim Copy()
    {
        return new CostModelClaim(
            Code,
            Name,
            StartsAt,
            Ratio,
            TenureMonths,
            Title,
            Country,
            Team,
            Cost,
            ManagerId,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
