namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive floor plan draft is in its life cycle.</summary>
public enum AdaptiveFloorPlanDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One adaptive floor plan draft, with what the planners need to act on it.</summary>
public sealed class AdaptiveFloorPlanDraft(string code, string name, int limit, Dictionary<string, string> attributes, string checksum, double minimum, AdaptiveFloorPlanDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Checksum { get; set; } = checksum;
    public double Minimum { get; set; } = minimum;
    public AdaptiveFloorPlanDraftStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive floor plan draft in status new.</summary>
    public static AdaptiveFloorPlanDraft NewAdaptiveFloorPlanDraft(string code, string name)
    {
        return new AdaptiveFloorPlanDraft(
            code,
            name,
            0,
            new Dictionary<string, string>(),
            "",
            0.0,
            AdaptiveFloorPlanDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveFloorPlanDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveFloorPlanDraftStatus next)
    {
        switch (Status)
        {
            case AdaptiveFloorPlanDraftStatus.New:
                return next == AdaptiveFloorPlanDraftStatus.Queued;
            case AdaptiveFloorPlanDraftStatus.Queued:
                switch (next)
                {
                    case AdaptiveFloorPlanDraftStatus.Done:
                    case AdaptiveFloorPlanDraftStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveFloorPlanDraftStatus.Running:
                return next == AdaptiveFloorPlanDraftStatus.Done;
            case AdaptiveFloorPlanDraftStatus.Done:
                return next == AdaptiveFloorPlanDraftStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdaptiveFloorPlanDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive floor plan draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double WeightedVolume()
    {
        return Math.Round(Minimum * (double)Limit, 2);
    }

    public AdaptiveFloorPlanDraft Copy()
    {
        return new AdaptiveFloorPlanDraft(Code, Name, Limit, Attributes, Checksum, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
