namespace Acme.Workforce.Assets.Assignment.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment snapshot is in its life cycle.</summary>
public enum AssignmentSnapshotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A assignment snapshot as the assignment team keeps it.</summary>
public sealed class AssignmentSnapshot(string code, string name, int priority, double hours, Dictionary<string, string> attributes, string skill, int updatedAt, string checksum, string title, int graceDays, AssignmentSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public double Hours { get; set; } = hours;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Skill { get; set; } = skill;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Checksum { get; set; } = checksum;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public AssignmentSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a assignment snapshot in status created.</summary>
    public static AssignmentSnapshot NewAssignmentSnapshot(string code, string name)
    {
        return new AssignmentSnapshot(
            code,
            name,
            0,
            0.0,
            new Dictionary<string, string>(),
            "",
            0,
            "",
            "",
            0,
            AssignmentSnapshotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentSnapshotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentSnapshotStatus next)
    {
        switch (Status)
        {
            case AssignmentSnapshotStatus.Created:
                switch (next)
                {
                    case AssignmentSnapshotStatus.Validated:
                        return true;
                }
                return false;
            case AssignmentSnapshotStatus.Validated:
                switch (next)
                {
                    case AssignmentSnapshotStatus.Published:
                        return true;
                }
                return false;
            case AssignmentSnapshotStatus.Published:
                switch (next)
                {
                    case AssignmentSnapshotStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"assignment snapshot"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedTotal()
    {
        return Math.Round((double)UpdatedAt * (double)Priority, 2);
    }

    public AssignmentSnapshot Copy()
    {
        return new AssignmentSnapshot(
            Code,
            Name,
            Priority,
            Hours,
            Attributes,
            Skill,
            UpdatedAt,
            Checksum,
            Title,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
