namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted adjustment draft is in its life cycle.</summary>
public enum WeightedAdjustmentDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One weighted adjustment draft, with what the planners need to act on it.</summary>
public sealed class WeightedAdjustmentDraft(string code, string label, double ceiling, Dictionary<string, string> attributes, bool approved, string notes, double weight, int batchSize, double fte, int leaveDays, string summary, WeightedAdjustmentDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Approved { get; set; } = approved;
    public string Notes { get; set; } = notes;
    public double Weight { get; set; } = weight;
    public int BatchSize { get; set; } = batchSize;
    public double Fte { get; set; } = fte;
    public int LeaveDays { get; set; } = leaveDays;
    public string Summary { get; set; } = summary;
    public WeightedAdjustmentDraftStatus Status { get; set; } = status;

    /// <summary>Starts a weighted adjustment draft in status new.</summary>
    public static WeightedAdjustmentDraft NewWeightedAdjustmentDraft(string code, string label)
    {
        return new WeightedAdjustmentDraft(
            code,
            label,
            0.0,
            new Dictionary<string, string>(),
            false,
            "",
            0.0,
            0,
            0.0,
            0,
            "",
            WeightedAdjustmentDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAdjustmentDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAdjustmentDraftStatus next)
    {
        switch (Status)
        {
            case WeightedAdjustmentDraftStatus.New:
                switch (next)
                {
                    case WeightedAdjustmentDraftStatus.Failed:
                        return true;
                }
                return false;
            case WeightedAdjustmentDraftStatus.Queued:
                return next == WeightedAdjustmentDraftStatus.Running;
            case WeightedAdjustmentDraftStatus.Running:
                return next == WeightedAdjustmentDraftStatus.Failed;
            case WeightedAdjustmentDraftStatus.Done:
                return next == WeightedAdjustmentDraftStatus.Failed;
        }
        return false;
    }

    public void MoveTo(WeightedAdjustmentDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted adjustment draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted adjustment draft"} {Code}";
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

    public double ProjectedValue()
    {
        return Math.Round(Weight + (double)LeaveDays, 2);
    }

    public WeightedAdjustmentDraft Copy()
    {
        return new WeightedAdjustmentDraft(
            Code,
            Label,
            Ceiling,
            Attributes,
            Approved,
            Notes,
            Weight,
            BatchSize,
            Fte,
            LeaveDays,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
