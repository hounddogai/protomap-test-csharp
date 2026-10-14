namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback garnishment plan is in its life cycle.</summary>
public enum FallbackGarnishmentPlanStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One fallback garnishment plan, with what the planners need to act on it.</summary>
public sealed class FallbackGarnishmentPlan(string code, string title, string grade, double weight, int endsAt, int updatedAt, double score, Dictionary<string, string> attributes, FallbackGarnishmentPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public double Weight { get; set; } = weight;
    public int EndsAt { get; set; } = endsAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Score { get; set; } = score;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public FallbackGarnishmentPlanStatus Status { get; set; } = status;

    /// <summary>Starts a fallback garnishment plan in status new.</summary>
    public static FallbackGarnishmentPlan NewFallbackGarnishmentPlan(string code, string title)
    {
        return new FallbackGarnishmentPlan(
            code,
            title,
            "",
            0.0,
            0,
            0,
            0.0,
            new Dictionary<string, string>(),
            FallbackGarnishmentPlanStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackGarnishmentPlanStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackGarnishmentPlanStatus next)
    {
        switch (Status)
        {
            case FallbackGarnishmentPlanStatus.New:
                return next == FallbackGarnishmentPlanStatus.Failed;
            case FallbackGarnishmentPlanStatus.Queued:
                switch (next)
                {
                    case FallbackGarnishmentPlanStatus.Running:
                        return true;
                }
                return false;
            case FallbackGarnishmentPlanStatus.Running:
                switch (next)
                {
                    case FallbackGarnishmentPlanStatus.Done:
                        return true;
                }
                return false;
            case FallbackGarnishmentPlanStatus.Done:
                return next == FallbackGarnishmentPlanStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FallbackGarnishmentPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback garnishment plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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
        return Math.Round(Weight - (double)UpdatedAt, 2);
    }

    public FallbackGarnishmentPlan Copy()
    {
        return new FallbackGarnishmentPlan(Code, Title, Grade, Weight, EndsAt, UpdatedAt, Score, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
