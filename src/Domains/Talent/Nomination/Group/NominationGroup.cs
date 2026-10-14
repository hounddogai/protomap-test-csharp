namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination group is in its life cycle.</summary>
public enum NominationGroupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a nomination group and its current status.</summary>
public sealed class NominationGroup(string code, string label, string costCenter, int createdAt, int limit, double score, string team, string contactEmail, string unit, string employeeId, double hours, NominationGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public int CreatedAt { get; set; } = createdAt;
    public int Limit { get; set; } = limit;
    public double Score { get; set; } = score;
    public string Team { get; set; } = team;
    public string ContactEmail { get; set; } = contactEmail;
    public string Unit { get; set; } = unit;
    public string EmployeeId { get; set; } = employeeId;
    public double Hours { get; set; } = hours;
    public NominationGroupStatus Status { get; set; } = status;

    /// <summary>Starts a nomination group in status draft.</summary>
    public static NominationGroup NewNominationGroup(string code, string label)
    {
        return new NominationGroup(code, label, "", 0, 0, 0.0, "", "", "", "", 0.0, NominationGroupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationGroupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationGroupStatus next)
    {
        switch (Status)
        {
            case NominationGroupStatus.Draft:
                switch (next)
                {
                    case NominationGroupStatus.Pending:
                    case NominationGroupStatus.Closed:
                        return true;
                }
                return false;
            case NominationGroupStatus.Pending:
                switch (next)
                {
                    case NominationGroupStatus.Closed:
                        return true;
                }
                return false;
            case NominationGroupStatus.Active:
                return next == NominationGroupStatus.Suspended || next == NominationGroupStatus.Closed;
            case NominationGroupStatus.Suspended:
                switch (next)
                {
                    case NominationGroupStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"nomination group"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)CreatedAt + (double)Limit, 2);
    }

    public NominationGroup Copy()
    {
        return new NominationGroup(
            Code,
            Label,
            CostCenter,
            CreatedAt,
            Limit,
            Score,
            Team,
            ContactEmail,
            Unit,
            EmployeeId,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
