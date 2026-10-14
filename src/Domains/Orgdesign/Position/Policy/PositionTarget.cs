namespace Acme.Workforce.Orgdesign.Position.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position target is in its life cycle.</summary>
public enum PositionTargetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A position target as the position team keeps it.</summary>
public sealed class PositionTarget(string code, string name, string badge, string department, string checksum, int createdAt, int graceDays, PositionTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public string Department { get; set; } = department;
    public string Checksum { get; set; } = checksum;
    public int CreatedAt { get; set; } = createdAt;
    public int GraceDays { get; set; } = graceDays;
    public PositionTargetStatus Status { get; set; } = status;

    /// <summary>Starts a position target in status created.</summary>
    public static PositionTarget NewPositionTarget(string code, string name)
    {
        return new PositionTarget(code, name, "", "", "", 0, 0, PositionTargetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionTargetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionTargetStatus next)
    {
        switch (Status)
        {
            case PositionTargetStatus.Created:
                switch (next)
                {
                    case PositionTargetStatus.Published:
                    case PositionTargetStatus.Retired:
                        return true;
                }
                return false;
            case PositionTargetStatus.Validated:
                switch (next)
                {
                    case PositionTargetStatus.Retired:
                        return true;
                }
                return false;
            case PositionTargetStatus.Published:
                return next == PositionTargetStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PositionTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round((double)CreatedAt * (double)GraceDays, 2);
    }

    public PositionTarget Copy()
    {
        return new PositionTarget(Code, Name, Badge, Department, Checksum, CreatedAt, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
