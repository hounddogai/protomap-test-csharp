namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive career path rule is in its life cycle.</summary>
public enum AdaptiveCareerPathRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a adaptive career path rule and its current status.</summary>
public sealed class AdaptiveCareerPathRule(string code, string label, double budget, double fte, int quantity, AdaptiveCareerPathRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public double Fte { get; set; } = fte;
    public int Quantity { get; set; } = quantity;
    public AdaptiveCareerPathRuleStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive career path rule in status idle.</summary>
    public static AdaptiveCareerPathRule NewAdaptiveCareerPathRule(string code, string label)
    {
        return new AdaptiveCareerPathRule(code, label, 0.0, 0.0, 0, AdaptiveCareerPathRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveCareerPathRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveCareerPathRuleStatus next)
    {
        switch (Status)
        {
            case AdaptiveCareerPathRuleStatus.Idle:
                return next == AdaptiveCareerPathRuleStatus.Ready || next == AdaptiveCareerPathRuleStatus.Draining;
            case AdaptiveCareerPathRuleStatus.Warming:
                return next == AdaptiveCareerPathRuleStatus.Draining;
            case AdaptiveCareerPathRuleStatus.Ready:
                return next == AdaptiveCareerPathRuleStatus.Stopped;
            case AdaptiveCareerPathRuleStatus.Draining:
                return next == AdaptiveCareerPathRuleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdaptiveCareerPathRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive career path rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Budget * (double)Quantity, 2);
    }

    public AdaptiveCareerPathRule Copy()
    {
        return new AdaptiveCareerPathRule(Code, Label, Budget, Fte, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
