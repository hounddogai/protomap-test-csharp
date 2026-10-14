namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary position plan is in its life cycle.</summary>
public enum PrimaryPositionPlanStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One primary position plan, with what the planners need to act on it.</summary>
public sealed class PrimaryPositionPlan(string code, string title, string country, bool archived, string checksum, int revision, PrimaryPositionPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public bool Archived { get; set; } = archived;
    public string Checksum { get; set; } = checksum;
    public int Revision { get; set; } = revision;
    public PrimaryPositionPlanStatus Status { get; set; } = status;

    /// <summary>Starts a primary position plan in status created.</summary>
    public static PrimaryPositionPlan NewPrimaryPositionPlan(string code, string title)
    {
        return new PrimaryPositionPlan(code, title, "", false, "", 0, PrimaryPositionPlanStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPositionPlanStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPositionPlanStatus next)
    {
        switch (Status)
        {
            case PrimaryPositionPlanStatus.Created:
                return next == PrimaryPositionPlanStatus.Retired;
            case PrimaryPositionPlanStatus.Validated:
                return next == PrimaryPositionPlanStatus.Retired;
            case PrimaryPositionPlanStatus.Published:
                return next == PrimaryPositionPlanStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PrimaryPositionPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary position plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public PrimaryPositionPlan Copy()
    {
        return new PrimaryPositionPlan(Code, Title, Country, Archived, Checksum, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
