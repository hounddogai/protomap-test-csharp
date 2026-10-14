namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction record is in its life cycle.</summary>
public enum DeductionRecordStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A deduction record. Its code is unique within a tenant.</summary>
public sealed class DeductionRecord(string code, string label, Dictionary<string, string> attributes, int createdAt, List<string> tags, double score, string segment, string team, string checksum, DeductionRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int CreatedAt { get; set; } = createdAt;
    public List<string> Tags { get; set; } = tags;
    public double Score { get; set; } = score;
    public string Segment { get; set; } = segment;
    public string Team { get; set; } = team;
    public string Checksum { get; set; } = checksum;
    public DeductionRecordStatus Status { get; set; } = status;

    /// <summary>Starts a deduction record in status idle.</summary>
    public static DeductionRecord NewDeductionRecord(string code, string label)
    {
        return new DeductionRecord(
            code,
            label,
            new Dictionary<string, string>(),
            0,
            new List<string>(),
            0.0,
            "",
            "",
            "",
            DeductionRecordStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionRecordStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionRecordStatus next)
    {
        switch (Status)
        {
            case DeductionRecordStatus.Idle:
                return next == DeductionRecordStatus.Draining;
            case DeductionRecordStatus.Warming:
                return next == DeductionRecordStatus.Stopped;
            case DeductionRecordStatus.Ready:
                switch (next)
                {
                    case DeductionRecordStatus.Draining:
                    case DeductionRecordStatus.Stopped:
                        return true;
                }
                return false;
            case DeductionRecordStatus.Draining:
                switch (next)
                {
                    case DeductionRecordStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"deduction record"} {Code}";
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
        return Math.Round((double)CreatedAt - Score, 2);
    }

    public DeductionRecord Copy()
    {
        return new DeductionRecord(Code, Label, Attributes, CreatedAt, Tags, Score, Segment, Team, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
