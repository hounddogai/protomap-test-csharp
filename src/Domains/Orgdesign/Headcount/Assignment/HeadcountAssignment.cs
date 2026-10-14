namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount assignment is in its life cycle.</summary>
public enum HeadcountAssignmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A headcount assignment. Its code is unique within a tenant.</summary>
public sealed class HeadcountAssignment(string code, string name, string team, string contactEmail, string segment, double rating, string managerId, HeadcountAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public string ContactEmail { get; set; } = contactEmail;
    public string Segment { get; set; } = segment;
    public double Rating { get; set; } = rating;
    public string ManagerId { get; set; } = managerId;
    public HeadcountAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a headcount assignment in status requested.</summary>
    public static HeadcountAssignment NewHeadcountAssignment(string code, string name)
    {
        return new HeadcountAssignment(code, name, "", "", "", 0.0, "", HeadcountAssignmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountAssignmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountAssignmentStatus next)
    {
        switch (Status)
        {
            case HeadcountAssignmentStatus.Requested:
                return next == HeadcountAssignmentStatus.Granted;
            case HeadcountAssignmentStatus.Granted:
                switch (next)
                {
                    case HeadcountAssignmentStatus.Expired:
                        return true;
                }
                return false;
            case HeadcountAssignmentStatus.Revoked:
                switch (next)
                {
                    case HeadcountAssignmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"headcount assignment"} {Code}";
    }

    public double MeasuredValue()
    {
        return Rating;
    }

    public HeadcountAssignment Copy()
    {
        return new HeadcountAssignment(Code, Name, Team, ContactEmail, Segment, Rating, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
