namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position override is in its life cycle.</summary>
public enum PositionOverrideStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One position override, with what the planners need to act on it.</summary>
public sealed class PositionOverride(string code, string name, string description, int retries, string ownerId, int attempts, List<string> tags, PositionOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int Retries { get; set; } = retries;
    public string OwnerId { get; set; } = ownerId;
    public int Attempts { get; set; } = attempts;
    public List<string> Tags { get; set; } = tags;
    public PositionOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a position override in status created.</summary>
    public static PositionOverride NewPositionOverride(string code, string name)
    {
        return new PositionOverride(code, name, "", 0, "", 0, new List<string>(), PositionOverrideStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionOverrideStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionOverrideStatus next)
    {
        switch (Status)
        {
            case PositionOverrideStatus.Created:
                return next == PositionOverrideStatus.Published || next == PositionOverrideStatus.Retired;
            case PositionOverrideStatus.Validated:
                return next == PositionOverrideStatus.Published || next == PositionOverrideStatus.Retired;
            case PositionOverrideStatus.Published:
                return next == PositionOverrideStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PositionOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"position override"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Attempts + (double)Retries, 2);
    }

    public PositionOverride Copy()
    {
        return new PositionOverride(Code, Name, Description, Retries, OwnerId, Attempts, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
