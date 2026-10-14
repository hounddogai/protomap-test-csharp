namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary scenario segment is in its life cycle.</summary>
public enum PrimaryScenarioSegmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A primary scenario segment. Its code is unique within a tenant.</summary>
public sealed class PrimaryScenarioSegment(string code, string title, int durationMinutes, int revision, bool pinned, PrimaryScenarioSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Revision { get; set; } = revision;
    public bool Pinned { get; set; } = pinned;
    public PrimaryScenarioSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a primary scenario segment in status draft.</summary>
    public static PrimaryScenarioSegment NewPrimaryScenarioSegment(string code, string title)
    {
        return new PrimaryScenarioSegment(code, title, 0, 0, false, PrimaryScenarioSegmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryScenarioSegmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryScenarioSegmentStatus next)
    {
        switch (Status)
        {
            case PrimaryScenarioSegmentStatus.Draft:
                switch (next)
                {
                    case PrimaryScenarioSegmentStatus.Active:
                    case PrimaryScenarioSegmentStatus.Suspended:
                        return true;
                }
                return false;
            case PrimaryScenarioSegmentStatus.Pending:
                return next == PrimaryScenarioSegmentStatus.Active || next == PrimaryScenarioSegmentStatus.Suspended;
            case PrimaryScenarioSegmentStatus.Active:
                switch (next)
                {
                    case PrimaryScenarioSegmentStatus.Suspended:
                    case PrimaryScenarioSegmentStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryScenarioSegmentStatus.Suspended:
                switch (next)
                {
                    case PrimaryScenarioSegmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryScenarioSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary scenario segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)DurationMinutes - (double)Revision, 2);
    }

    public PrimaryScenarioSegment Copy()
    {
        return new PrimaryScenarioSegment(Code, Title, DurationMinutes, Revision, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
