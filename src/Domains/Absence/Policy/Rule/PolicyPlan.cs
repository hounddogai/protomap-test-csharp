namespace Acme.Workforce.Absence.Policy.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy plan is in its life cycle.</summary>
public enum PolicyPlanStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One policy plan, with what the planners need to act on it.</summary>
public sealed class PolicyPlan(string code, string label, bool archived, string title, int updatedAt, string badge, int retries, string category, string currency, string locale, PolicyPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Badge { get; set; } = badge;
    public int Retries { get; set; } = retries;
    public string Category { get; set; } = category;
    public string Currency { get; set; } = currency;
    public string Locale { get; set; } = locale;
    public PolicyPlanStatus Status { get; set; } = status;

    /// <summary>Starts a policy plan in status new.</summary>
    public static PolicyPlan NewPolicyPlan(string code, string label)
    {
        return new PolicyPlan(code, label, false, "", 0, "", 0, "", "", "", PolicyPlanStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyPlanStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyPlanStatus next)
    {
        switch (Status)
        {
            case PolicyPlanStatus.New:
                switch (next)
                {
                    case PolicyPlanStatus.Queued:
                        return true;
                }
                return false;
            case PolicyPlanStatus.Queued:
                switch (next)
                {
                    case PolicyPlanStatus.Done:
                    case PolicyPlanStatus.Failed:
                        return true;
                }
                return false;
            case PolicyPlanStatus.Running:
                switch (next)
                {
                    case PolicyPlanStatus.Done:
                        return true;
                }
                return false;
            case PolicyPlanStatus.Done:
                return next == PolicyPlanStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PolicyPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Retries * (double)UpdatedAt, 2);
    }

    public PolicyPlan Copy()
    {
        return new PolicyPlan(
            Code,
            Label,
            Archived,
            Title,
            UpdatedAt,
            Badge,
            Retries,
            Category,
            Currency,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
