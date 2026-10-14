namespace Acme.Workforce.Assets.AssetReturn.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled asset return batch is in its life cycle.</summary>
public enum ScheduledAssetReturnBatchStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A scheduled asset return batch. Its code is unique within a tenant.</summary>
public sealed class ScheduledAssetReturnBatch(string code, string name, double budget, string grade, string notes, Dictionary<string, string> attributes, int durationMinutes, string contactEmail, int version, ScheduledAssetReturnBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public string Grade { get; set; } = grade;
    public string Notes { get; set; } = notes;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ContactEmail { get; set; } = contactEmail;
    public int Version { get; set; } = version;
    public ScheduledAssetReturnBatchStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled asset return batch in status draft.</summary>
    public static ScheduledAssetReturnBatch NewScheduledAssetReturnBatch(string code, string name)
    {
        return new ScheduledAssetReturnBatch(
            code,
            name,
            0.0,
            "",
            "",
            new Dictionary<string, string>(),
            0,
            "",
            0,
            ScheduledAssetReturnBatchStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAssetReturnBatchStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAssetReturnBatchStatus next)
    {
        switch (Status)
        {
            case ScheduledAssetReturnBatchStatus.Draft:
                return next == ScheduledAssetReturnBatchStatus.Pending || next == ScheduledAssetReturnBatchStatus.Closed;
            case ScheduledAssetReturnBatchStatus.Pending:
                return next == ScheduledAssetReturnBatchStatus.Suspended;
            case ScheduledAssetReturnBatchStatus.Active:
                switch (next)
                {
                    case ScheduledAssetReturnBatchStatus.Suspended:
                    case ScheduledAssetReturnBatchStatus.Closed:
                        return true;
                }
                return false;
            case ScheduledAssetReturnBatchStatus.Suspended:
                return next == ScheduledAssetReturnBatchStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ScheduledAssetReturnBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled asset return batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double NetExposure()
    {
        return Math.Round(Budget + (double)DurationMinutes, 2);
    }

    public ScheduledAssetReturnBatch Copy()
    {
        return new ScheduledAssetReturnBatch(
            Code,
            Name,
            Budget,
            Grade,
            Notes,
            Attributes,
            DurationMinutes,
            ContactEmail,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
