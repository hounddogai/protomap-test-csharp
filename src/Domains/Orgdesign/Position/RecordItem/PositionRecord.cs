namespace Acme.Workforce.Orgdesign.Position.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position record is in its life cycle.</summary>
public enum PositionRecordStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A position record as the position team keeps it.</summary>
public sealed class PositionRecord(string code, string name, string grade, Dictionary<string, string> attributes, string source, int attempts, int revision, int endsAt, double budget, int version, bool pinned, PositionRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Source { get; set; } = source;
    public int Attempts { get; set; } = attempts;
    public int Revision { get; set; } = revision;
    public int EndsAt { get; set; } = endsAt;
    public double Budget { get; set; } = budget;
    public int Version { get; set; } = version;
    public bool Pinned { get; set; } = pinned;
    public PositionRecordStatus Status { get; set; } = status;

    /// <summary>Starts a position record in status idle.</summary>
    public static PositionRecord NewPositionRecord(string code, string name)
    {
        return new PositionRecord(
            code,
            name,
            "",
            new Dictionary<string, string>(),
            "",
            0,
            0,
            0,
            0.0,
            0,
            false,
            PositionRecordStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionRecordStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionRecordStatus next)
    {
        switch (Status)
        {
            case PositionRecordStatus.Idle:
                switch (next)
                {
                    case PositionRecordStatus.Ready:
                    case PositionRecordStatus.Draining:
                        return true;
                }
                return false;
            case PositionRecordStatus.Warming:
                switch (next)
                {
                    case PositionRecordStatus.Draining:
                        return true;
                }
                return false;
            case PositionRecordStatus.Ready:
                switch (next)
                {
                    case PositionRecordStatus.Draining:
                    case PositionRecordStatus.Stopped:
                        return true;
                }
                return false;
            case PositionRecordStatus.Draining:
                switch (next)
                {
                    case PositionRecordStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"position record"} {Code}";
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

    public double AdjustedVolume()
    {
        return Math.Round((double)Revision - Budget, 2);
    }

    public PositionRecord Copy()
    {
        return new PositionRecord(
            Code,
            Name,
            Grade,
            Attributes,
            Source,
            Attempts,
            Revision,
            EndsAt,
            Budget,
            Version,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
