namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped position assignment is in its life cycle.</summary>
public enum CappedPositionAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A capped position assignment. Its code is unique within a tenant.</summary>
public sealed class CappedPositionAssignment(string code, string name, string locale, bool visible, string team, string region, double ratio, int version, string reviewer, double amount, double rate, CappedPositionAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public bool Visible { get; set; } = visible;
    public string Team { get; set; } = team;
    public string Region { get; set; } = region;
    public double Ratio { get; set; } = ratio;
    public int Version { get; set; } = version;
    public string Reviewer { get; set; } = reviewer;
    public double Amount { get; set; } = amount;
    public double Rate { get; set; } = rate;
    public CappedPositionAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a capped position assignment in status open.</summary>
    public static CappedPositionAssignment NewCappedPositionAssignment(string code, string name)
    {
        return new CappedPositionAssignment(
            code,
            name,
            "",
            false,
            "",
            "",
            0.0,
            0,
            "",
            0.0,
            0.0,
            CappedPositionAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPositionAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPositionAssignmentStatus next)
    {
        switch (Status)
        {
            case CappedPositionAssignmentStatus.Open:
                switch (next)
                {
                    case CappedPositionAssignmentStatus.InReview:
                    case CappedPositionAssignmentStatus.Archived:
                        return true;
                }
                return false;
            case CappedPositionAssignmentStatus.InReview:
                return next == CappedPositionAssignmentStatus.Resolved;
            case CappedPositionAssignmentStatus.Resolved:
                switch (next)
                {
                    case CappedPositionAssignmentStatus.Reopened:
                        return true;
                }
                return false;
            case CappedPositionAssignmentStatus.Reopened:
                switch (next)
                {
                    case CappedPositionAssignmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedPositionAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped position assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"capped position assignment"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Ratio - Rate, 2);
    }

    public CappedPositionAssignment Copy()
    {
        return new CappedPositionAssignment(
            Code,
            Name,
            Locale,
            Visible,
            Team,
            Region,
            Ratio,
            Version,
            Reviewer,
            Amount,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
