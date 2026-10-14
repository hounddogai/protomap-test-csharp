namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary headcount assignment is in its life cycle.</summary>
public enum PrimaryHeadcountAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One primary headcount assignment, with what the planners need to act on it.</summary>
public sealed class PrimaryHeadcountAssignment(string code, string title, double minimum, string ownerId, int batchSize, string channel, int tenureMonths, int capacity, bool archived, string description, PrimaryHeadcountAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string OwnerId { get; set; } = ownerId;
    public int BatchSize { get; set; } = batchSize;
    public string Channel { get; set; } = channel;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Capacity { get; set; } = capacity;
    public bool Archived { get; set; } = archived;
    public string Description { get; set; } = description;
    public PrimaryHeadcountAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a primary headcount assignment in status proposed.</summary>
    public static PrimaryHeadcountAssignment NewPrimaryHeadcountAssignment(string code, string title)
    {
        return new PrimaryHeadcountAssignment(
            code,
            title,
            0.0,
            "",
            0,
            "",
            0,
            0,
            false,
            "",
            PrimaryHeadcountAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryHeadcountAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryHeadcountAssignmentStatus next)
    {
        switch (Status)
        {
            case PrimaryHeadcountAssignmentStatus.Proposed:
                return next == PrimaryHeadcountAssignmentStatus.Approved;
            case PrimaryHeadcountAssignmentStatus.Approved:
                switch (next)
                {
                    case PrimaryHeadcountAssignmentStatus.Rejected:
                        return true;
                }
                return false;
            case PrimaryHeadcountAssignmentStatus.Rejected:
                switch (next)
                {
                    case PrimaryHeadcountAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryHeadcountAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary headcount assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)TenureMonths + Minimum, 2);
    }

    public PrimaryHeadcountAssignment Copy()
    {
        return new PrimaryHeadcountAssignment(
            Code,
            Title,
            Minimum,
            OwnerId,
            BatchSize,
            Channel,
            TenureMonths,
            Capacity,
            Archived,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
