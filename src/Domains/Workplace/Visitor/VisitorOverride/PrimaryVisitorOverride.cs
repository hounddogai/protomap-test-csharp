namespace Acme.Workforce.Workplace.Visitor.VisitorOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary visitor override is in its life cycle.</summary>
public enum PrimaryVisitorOverrideStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A primary visitor override as the visitor team keeps it.</summary>
public sealed class PrimaryVisitorOverride(string code, string name, int durationMinutes, string category, double minimum, int createdAt, string ownerId, string badge, int revision, PrimaryVisitorOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Category { get; set; } = category;
    public double Minimum { get; set; } = minimum;
    public int CreatedAt { get; set; } = createdAt;
    public string OwnerId { get; set; } = ownerId;
    public string Badge { get; set; } = badge;
    public int Revision { get; set; } = revision;
    public PrimaryVisitorOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a primary visitor override in status draft.</summary>
    public static PrimaryVisitorOverride NewPrimaryVisitorOverride(string code, string name)
    {
        return new PrimaryVisitorOverride(code, name, 0, "", 0.0, 0, "", "", 0, PrimaryVisitorOverrideStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryVisitorOverrideStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryVisitorOverrideStatus next)
    {
        switch (Status)
        {
            case PrimaryVisitorOverrideStatus.Draft:
                switch (next)
                {
                    case PrimaryVisitorOverrideStatus.Pending:
                    case PrimaryVisitorOverrideStatus.Suspended:
                        return true;
                }
                return false;
            case PrimaryVisitorOverrideStatus.Pending:
                switch (next)
                {
                    case PrimaryVisitorOverrideStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryVisitorOverrideStatus.Active:
                return next == PrimaryVisitorOverrideStatus.Suspended || next == PrimaryVisitorOverrideStatus.Closed;
            case PrimaryVisitorOverrideStatus.Suspended:
                switch (next)
                {
                    case PrimaryVisitorOverrideStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryVisitorOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary visitor override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Minimum - (double)CreatedAt, 2);
    }

    public PrimaryVisitorOverride Copy()
    {
        return new PrimaryVisitorOverride(
            Code,
            Name,
            DurationMinutes,
            Category,
            Minimum,
            CreatedAt,
            OwnerId,
            Badge,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
