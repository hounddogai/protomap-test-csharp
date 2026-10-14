namespace Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive scenario template is in its life cycle.</summary>
public enum AdaptiveScenarioTemplateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A adaptive scenario template as the scenario team keeps it.</summary>
public sealed class AdaptiveScenarioTemplate(string code, string name, int leaveDays, int batchSize, int endsAt, int sequence, int createdAt, string title, int startsAt, string summary, AdaptiveScenarioTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public int BatchSize { get; set; } = batchSize;
    public int EndsAt { get; set; } = endsAt;
    public int Sequence { get; set; } = sequence;
    public int CreatedAt { get; set; } = createdAt;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public string Summary { get; set; } = summary;
    public AdaptiveScenarioTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive scenario template in status idle.</summary>
    public static AdaptiveScenarioTemplate NewAdaptiveScenarioTemplate(string code, string name)
    {
        return new AdaptiveScenarioTemplate(
            code,
            name,
            0,
            0,
            0,
            0,
            0,
            "",
            0,
            "",
            AdaptiveScenarioTemplateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveScenarioTemplateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveScenarioTemplateStatus next)
    {
        switch (Status)
        {
            case AdaptiveScenarioTemplateStatus.Idle:
                return next == AdaptiveScenarioTemplateStatus.Draining || next == AdaptiveScenarioTemplateStatus.Stopped;
            case AdaptiveScenarioTemplateStatus.Warming:
                return next == AdaptiveScenarioTemplateStatus.Draining;
            case AdaptiveScenarioTemplateStatus.Ready:
                return next == AdaptiveScenarioTemplateStatus.Draining || next == AdaptiveScenarioTemplateStatus.Stopped;
            case AdaptiveScenarioTemplateStatus.Draining:
                return next == AdaptiveScenarioTemplateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdaptiveScenarioTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive scenario template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)EndsAt * (double)StartsAt, 2);
    }

    public AdaptiveScenarioTemplate Copy()
    {
        return new AdaptiveScenarioTemplate(
            Code,
            Name,
            LeaveDays,
            BatchSize,
            EndsAt,
            Sequence,
            CreatedAt,
            Title,
            StartsAt,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
