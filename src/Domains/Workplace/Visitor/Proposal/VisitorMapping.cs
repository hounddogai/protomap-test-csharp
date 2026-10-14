namespace Acme.Workforce.Workplace.Visitor.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor mapping is in its life cycle.</summary>
public enum VisitorMappingStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One visitor mapping, with what the planners need to act on it.</summary>
public sealed class VisitorMapping(string code, string name, string externalRef, double fte, int createdAt, double budget, string timezone, VisitorMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public double Fte { get; set; } = fte;
    public int CreatedAt { get; set; } = createdAt;
    public double Budget { get; set; } = budget;
    public string Timezone { get; set; } = timezone;
    public VisitorMappingStatus Status { get; set; } = status;

    /// <summary>Starts a visitor mapping in status open.</summary>
    public static VisitorMapping NewVisitorMapping(string code, string name)
    {
        return new VisitorMapping(code, name, "", 0.0, 0, 0.0, "", VisitorMappingStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorMappingStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorMappingStatus next)
    {
        switch (Status)
        {
            case VisitorMappingStatus.Open:
                return next == VisitorMappingStatus.Archived;
            case VisitorMappingStatus.InReview:
                return next == VisitorMappingStatus.Reopened;
            case VisitorMappingStatus.Resolved:
                return next == VisitorMappingStatus.Reopened || next == VisitorMappingStatus.Archived;
            case VisitorMappingStatus.Reopened:
                return next == VisitorMappingStatus.Archived;
        }
        return false;
    }

    public void MoveTo(VisitorMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedValue()
    {
        return Math.Round(Fte + (double)CreatedAt, 2);
    }

    public VisitorMapping Copy()
    {
        return new VisitorMapping(Code, Name, ExternalRef, Fte, CreatedAt, Budget, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
