namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg snapshot is in its life cycle.</summary>
public enum ReorgSnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a reorg snapshot and its current status.</summary>
public sealed class ReorgSnapshot(string code, string name, double margin, string region, bool archived, Dictionary<string, string> attributes, double cost, string skill, ReorgSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public string Region { get; set; } = region;
    public bool Archived { get; set; } = archived;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Cost { get; set; } = cost;
    public string Skill { get; set; } = skill;
    public ReorgSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a reorg snapshot in status new.</summary>
    public static ReorgSnapshot NewReorgSnapshot(string code, string name)
    {
        return new ReorgSnapshot(
            code,
            name,
            0.0,
            "",
            false,
            new Dictionary<string, string>(),
            0.0,
            "",
            ReorgSnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgSnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgSnapshotStatus next)
    {
        switch (Status)
        {
            case ReorgSnapshotStatus.New:
                switch (next)
                {
                    case ReorgSnapshotStatus.Queued:
                        return true;
                }
                return false;
            case ReorgSnapshotStatus.Queued:
                switch (next)
                {
                    case ReorgSnapshotStatus.Running:
                    case ReorgSnapshotStatus.Done:
                        return true;
                }
                return false;
            case ReorgSnapshotStatus.Running:
                switch (next)
                {
                    case ReorgSnapshotStatus.Done:
                        return true;
                }
                return false;
            case ReorgSnapshotStatus.Done:
                return next == ReorgSnapshotStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ReorgSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"reorg snapshot"} {Code}";
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

    public double NetVolume()
    {
        return Math.Round(Margin - Cost, 2);
    }

    public ReorgSnapshot Copy()
    {
        return new ReorgSnapshot(Code, Name, Margin, Region, Archived, Attributes, Cost, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
