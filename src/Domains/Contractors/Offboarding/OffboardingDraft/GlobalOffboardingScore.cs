namespace Acme.Workforce.Contractors.Offboarding.OffboardingDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global offboarding score is in its life cycle.</summary>
public enum GlobalOffboardingScoreStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A global offboarding score. Its code is unique within a tenant.</summary>
public sealed class GlobalOffboardingScore(string code, string title, double floorValue, double ratio, string department, string source, string segment, GlobalOffboardingScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public double Ratio { get; set; } = ratio;
    public string Department { get; set; } = department;
    public string Source { get; set; } = source;
    public string Segment { get; set; } = segment;
    public GlobalOffboardingScoreStatus Status { get; set; } = status;

    /// <summary>Starts a global offboarding score in status planned.</summary>
    public static GlobalOffboardingScore NewGlobalOffboardingScore(string code, string title)
    {
        return new GlobalOffboardingScore(code, title, 0.0, 0.0, "", "", "", GlobalOffboardingScoreStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalOffboardingScoreStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalOffboardingScoreStatus next)
    {
        switch (Status)
        {
            case GlobalOffboardingScoreStatus.Planned:
                return next == GlobalOffboardingScoreStatus.Scheduled || next == GlobalOffboardingScoreStatus.Cancelled;
            case GlobalOffboardingScoreStatus.Scheduled:
                switch (next)
                {
                    case GlobalOffboardingScoreStatus.Completed:
                        return true;
                }
                return false;
            case GlobalOffboardingScoreStatus.InProgress:
                return next == GlobalOffboardingScoreStatus.Completed || next == GlobalOffboardingScoreStatus.Cancelled;
            case GlobalOffboardingScoreStatus.Completed:
                switch (next)
                {
                    case GlobalOffboardingScoreStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalOffboardingScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global offboarding score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global offboarding score"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(FloorValue * Ratio, 2);
    }

    public GlobalOffboardingScore Copy()
    {
        return new GlobalOffboardingScore(Code, Title, FloorValue, Ratio, Department, Source, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
