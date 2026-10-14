namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered pool rule is in its life cycle.</summary>
public enum TieredPoolRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a tiered pool rule and its current status.</summary>
public sealed class TieredPoolRule(string code, string label, string department, double ceiling, string channel, string summary, string jobCode, string region, string segment, int updatedAt, TieredPoolRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public double Ceiling { get; set; } = ceiling;
    public string Channel { get; set; } = channel;
    public string Summary { get; set; } = summary;
    public string JobCode { get; set; } = jobCode;
    public string Region { get; set; } = region;
    public string Segment { get; set; } = segment;
    public int UpdatedAt { get; set; } = updatedAt;
    public TieredPoolRuleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered pool rule in status new.</summary>
    public static TieredPoolRule NewTieredPoolRule(string code, string label)
    {
        return new TieredPoolRule(code, label, "", 0.0, "", "", "", "", "", 0, TieredPoolRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPoolRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPoolRuleStatus next)
    {
        switch (Status)
        {
            case TieredPoolRuleStatus.New:
                switch (next)
                {
                    case TieredPoolRuleStatus.Failed:
                        return true;
                }
                return false;
            case TieredPoolRuleStatus.Queued:
                switch (next)
                {
                    case TieredPoolRuleStatus.Done:
                        return true;
                }
                return false;
            case TieredPoolRuleStatus.Running:
                return next == TieredPoolRuleStatus.Done || next == TieredPoolRuleStatus.Failed;
            case TieredPoolRuleStatus.Done:
                return next == TieredPoolRuleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(TieredPoolRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered pool rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"tiered pool rule"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Ceiling * (double)UpdatedAt, 2);
    }

    public TieredPoolRule Copy()
    {
        return new TieredPoolRule(
            Code,
            Label,
            Department,
            Ceiling,
            Channel,
            Summary,
            JobCode,
            Region,
            Segment,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
