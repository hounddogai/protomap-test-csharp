namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work snapshot is in its life cycle.</summary>
public enum StatementOfWorkSnapshotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One statement of work snapshot, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkSnapshot(string code, string label, string team, Dictionary<string, string> attributes, double ratio, double cost, int revision, double hours, double floorValue, StatementOfWorkSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Ratio { get; set; } = ratio;
    public double Cost { get; set; } = cost;
    public int Revision { get; set; } = revision;
    public double Hours { get; set; } = hours;
    public double FloorValue { get; set; } = floorValue;
    public StatementOfWorkSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work snapshot in status created.</summary>
    public static StatementOfWorkSnapshot NewStatementOfWorkSnapshot(string code, string label)
    {
        return new StatementOfWorkSnapshot(
            code,
            label,
            "",
            new Dictionary<string, string>(),
            0.0,
            0.0,
            0,
            0.0,
            0.0,
            StatementOfWorkSnapshotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkSnapshotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkSnapshotStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkSnapshotStatus.Created:
                return next == StatementOfWorkSnapshotStatus.Published || next == StatementOfWorkSnapshotStatus.Retired;
            case StatementOfWorkSnapshotStatus.Validated:
                return next == StatementOfWorkSnapshotStatus.Retired;
            case StatementOfWorkSnapshotStatus.Published:
                return next == StatementOfWorkSnapshotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double EffectiveLoad()
    {
        return Math.Round(Hours * (double)Revision, 2);
    }

    public StatementOfWorkSnapshot Copy()
    {
        return new StatementOfWorkSnapshot(
            Code,
            Label,
            Team,
            Attributes,
            Ratio,
            Cost,
            Revision,
            Hours,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
