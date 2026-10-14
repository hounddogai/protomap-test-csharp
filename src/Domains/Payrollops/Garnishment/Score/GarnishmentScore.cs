namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment score is in its life cycle.</summary>
public enum GarnishmentScoreStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A garnishment score as the garnishment team keeps it.</summary>
public sealed class GarnishmentScore(string code, string name, bool archived, string employeeId, int durationMinutes, string location, string phone, int updatedAt, int sequence, GarnishmentScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string EmployeeId { get; set; } = employeeId;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Location { get; set; } = location;
    public string Phone { get; set; } = phone;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Sequence { get; set; } = sequence;
    public GarnishmentScoreStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment score in status open.</summary>
    public static GarnishmentScore NewGarnishmentScore(string code, string name)
    {
        return new GarnishmentScore(code, name, false, "", 0, "", "", 0, 0, GarnishmentScoreStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentScoreStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentScoreStatus next)
    {
        switch (Status)
        {
            case GarnishmentScoreStatus.Open:
                switch (next)
                {
                    case GarnishmentScoreStatus.InReview:
                    case GarnishmentScoreStatus.Reopened:
                        return true;
                }
                return false;
            case GarnishmentScoreStatus.InReview:
                switch (next)
                {
                    case GarnishmentScoreStatus.Resolved:
                        return true;
                }
                return false;
            case GarnishmentScoreStatus.Resolved:
                switch (next)
                {
                    case GarnishmentScoreStatus.Reopened:
                    case GarnishmentScoreStatus.Archived:
                        return true;
                }
                return false;
            case GarnishmentScoreStatus.Reopened:
                switch (next)
                {
                    case GarnishmentScoreStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)UpdatedAt - (double)DurationMinutes, 2);
    }

    public GarnishmentScore Copy()
    {
        return new GarnishmentScore(
            Code,
            Name,
            Archived,
            EmployeeId,
            DurationMinutes,
            Location,
            Phone,
            UpdatedAt,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
