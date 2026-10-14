namespace Acme.Workforce.Workplace.Occupancy.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy agreement is in its life cycle.</summary>
public enum OccupancyAgreementStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A occupancy agreement as the occupancy team keeps it.</summary>
public sealed class OccupancyAgreement(string code, string title, string costCenter, int endsAt, List<string> tags, string team, int limit, OccupancyAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public int EndsAt { get; set; } = endsAt;
    public List<string> Tags { get; set; } = tags;
    public string Team { get; set; } = team;
    public int Limit { get; set; } = limit;
    public OccupancyAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy agreement in status proposed.</summary>
    public static OccupancyAgreement NewOccupancyAgreement(string code, string title)
    {
        return new OccupancyAgreement(
            code,
            title,
            "",
            0,
            new List<string>(),
            "",
            0,
            OccupancyAgreementStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyAgreementStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyAgreementStatus next)
    {
        switch (Status)
        {
            case OccupancyAgreementStatus.Proposed:
                return next == OccupancyAgreementStatus.Rejected;
            case OccupancyAgreementStatus.Approved:
                switch (next)
                {
                    case OccupancyAgreementStatus.Rejected:
                        return true;
                }
                return false;
            case OccupancyAgreementStatus.Rejected:
                switch (next)
                {
                    case OccupancyAgreementStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedExposure()
    {
        return Math.Round((double)EndsAt - (double)Limit, 2);
    }

    public OccupancyAgreement Copy()
    {
        return new OccupancyAgreement(Code, Title, CostCenter, EndsAt, Tags, Team, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
