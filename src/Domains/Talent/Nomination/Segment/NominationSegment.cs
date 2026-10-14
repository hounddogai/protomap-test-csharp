namespace Acme.Workforce.Talent.Nomination.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination segment is in its life cycle.</summary>
public enum NominationSegmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One nomination segment, with what the planners need to act on it.</summary>
public sealed class NominationSegment(string code, string title, string jobCode, string category, string checksum, string salaryBand, double budget, NominationSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public string Category { get; set; } = category;
    public string Checksum { get; set; } = checksum;
    public string SalaryBand { get; set; } = salaryBand;
    public double Budget { get; set; } = budget;
    public NominationSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a nomination segment in status draft.</summary>
    public static NominationSegment NewNominationSegment(string code, string title)
    {
        return new NominationSegment(code, title, "", "", "", "", 0.0, NominationSegmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationSegmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationSegmentStatus next)
    {
        switch (Status)
        {
            case NominationSegmentStatus.Draft:
                return next == NominationSegmentStatus.Closed;
            case NominationSegmentStatus.Pending:
                return next == NominationSegmentStatus.Closed;
            case NominationSegmentStatus.Active:
                return next == NominationSegmentStatus.Closed;
            case NominationSegmentStatus.Suspended:
                switch (next)
                {
                    case NominationSegmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public NominationSegment Copy()
    {
        return new NominationSegment(Code, Title, JobCode, Category, Checksum, SalaryBand, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
