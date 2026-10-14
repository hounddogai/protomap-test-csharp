namespace Acme.Workforce.Contractors.Offboarding.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding draft is in its life cycle.</summary>
public enum OffboardingDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a offboarding draft and its current status.</summary>
public sealed class OffboardingDraft(string code, string name, string title, string timezone, double score, int tenureMonths, double floorValue, double maximum, string summary, OffboardingDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public double Score { get; set; } = score;
    public int TenureMonths { get; set; } = tenureMonths;
    public double FloorValue { get; set; } = floorValue;
    public double Maximum { get; set; } = maximum;
    public string Summary { get; set; } = summary;
    public OffboardingDraftStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding draft in status new.</summary>
    public static OffboardingDraft NewOffboardingDraft(string code, string name)
    {
        return new OffboardingDraft(code, name, "", "", 0.0, 0, 0.0, 0.0, "", OffboardingDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingDraftStatus next)
    {
        switch (Status)
        {
            case OffboardingDraftStatus.New:
                switch (next)
                {
                    case OffboardingDraftStatus.Queued:
                    case OffboardingDraftStatus.Failed:
                        return true;
                }
                return false;
            case OffboardingDraftStatus.Queued:
                return next == OffboardingDraftStatus.Failed;
            case OffboardingDraftStatus.Running:
                return next == OffboardingDraftStatus.Failed;
            case OffboardingDraftStatus.Done:
                return next == OffboardingDraftStatus.Failed;
        }
        return false;
    }

    public void MoveTo(OffboardingDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"offboarding draft"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(FloorValue * Score, 2);
    }

    public OffboardingDraft Copy()
    {
        return new OffboardingDraft(
            Code,
            Name,
            Title,
            Timezone,
            Score,
            TenureMonths,
            FloorValue,
            Maximum,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
