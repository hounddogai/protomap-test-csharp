namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan variance is in its life cycle.</summary>
public enum SuccessionPlanVarianceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One succession plan variance, with what the planners need to act on it.</summary>
public sealed class SuccessionPlanVariance(string code, string name, string title, bool archived, string employeeId, int windowDays, int expiresAt, string label, string skill, double weight, double minimum, SuccessionPlanVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string EmployeeId { get; set; } = employeeId;
    public int WindowDays { get; set; } = windowDays;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public double Weight { get; set; } = weight;
    public double Minimum { get; set; } = minimum;
    public SuccessionPlanVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan variance in status idle.</summary>
    public static SuccessionPlanVariance NewSuccessionPlanVariance(string code, string name)
    {
        return new SuccessionPlanVariance(
            code,
            name,
            "",
            false,
            "",
            0,
            0,
            "",
            "",
            0.0,
            0.0,
            SuccessionPlanVarianceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanVarianceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanVarianceStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanVarianceStatus.Idle:
                return next == SuccessionPlanVarianceStatus.Draining || next == SuccessionPlanVarianceStatus.Stopped;
            case SuccessionPlanVarianceStatus.Warming:
                return next == SuccessionPlanVarianceStatus.Draining || next == SuccessionPlanVarianceStatus.Stopped;
            case SuccessionPlanVarianceStatus.Ready:
                return next == SuccessionPlanVarianceStatus.Draining || next == SuccessionPlanVarianceStatus.Stopped;
            case SuccessionPlanVarianceStatus.Draining:
                switch (next)
                {
                    case SuccessionPlanVarianceStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)WindowDays - Minimum, 2);
    }

    public SuccessionPlanVariance Copy()
    {
        return new SuccessionPlanVariance(
            Code,
            Name,
            Title,
            Archived,
            EmployeeId,
            WindowDays,
            ExpiresAt,
            Label,
            Skill,
            Weight,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
