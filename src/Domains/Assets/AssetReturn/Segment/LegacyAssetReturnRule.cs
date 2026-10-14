namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy asset return rule is in its life cycle.</summary>
public enum LegacyAssetReturnRuleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One legacy asset return rule, with what the planners need to act on it.</summary>
public sealed class LegacyAssetReturnRule(string code, string name, int attempts, string notes, string source, int graceDays, LegacyAssetReturnRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Notes { get; set; } = notes;
    public string Source { get; set; } = source;
    public int GraceDays { get; set; } = graceDays;
    public LegacyAssetReturnRuleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy asset return rule in status planned.</summary>
    public static LegacyAssetReturnRule NewLegacyAssetReturnRule(string code, string name)
    {
        return new LegacyAssetReturnRule(code, name, 0, "", "", 0, LegacyAssetReturnRuleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyAssetReturnRuleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyAssetReturnRuleStatus next)
    {
        switch (Status)
        {
            case LegacyAssetReturnRuleStatus.Planned:
                return next == LegacyAssetReturnRuleStatus.Cancelled;
            case LegacyAssetReturnRuleStatus.Scheduled:
                return next == LegacyAssetReturnRuleStatus.Completed || next == LegacyAssetReturnRuleStatus.Cancelled;
            case LegacyAssetReturnRuleStatus.InProgress:
                switch (next)
                {
                    case LegacyAssetReturnRuleStatus.Completed:
                        return true;
                }
                return false;
            case LegacyAssetReturnRuleStatus.Completed:
                switch (next)
                {
                    case LegacyAssetReturnRuleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyAssetReturnRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy asset return rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)GraceDays * (double)Attempts, 2);
    }

    public LegacyAssetReturnRule Copy()
    {
        return new LegacyAssetReturnRule(Code, Name, Attempts, Notes, Source, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
