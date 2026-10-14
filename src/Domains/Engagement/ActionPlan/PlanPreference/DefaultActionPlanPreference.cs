namespace Acme.Workforce.Engagement.ActionPlan.PlanPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default action plan preference is in its life cycle.</summary>
public enum DefaultActionPlanPreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a default action plan preference and its current status.</summary>
public sealed class DefaultActionPlanPreference(string code, string name, string source, bool pinned, int tenureMonths, int capacity, Dictionary<string, string> attributes, double fte, double amount, int retries, DefaultActionPlanPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public bool Pinned { get; set; } = pinned;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Capacity { get; set; } = capacity;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Fte { get; set; } = fte;
    public double Amount { get; set; } = amount;
    public int Retries { get; set; } = retries;
    public DefaultActionPlanPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a default action plan preference in status open.</summary>
    public static DefaultActionPlanPreference NewDefaultActionPlanPreference(string code, string name)
    {
        return new DefaultActionPlanPreference(
            code,
            name,
            "",
            false,
            0,
            0,
            new Dictionary<string, string>(),
            0.0,
            0.0,
            0,
            DefaultActionPlanPreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultActionPlanPreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultActionPlanPreferenceStatus next)
    {
        switch (Status)
        {
            case DefaultActionPlanPreferenceStatus.Open:
                switch (next)
                {
                    case DefaultActionPlanPreferenceStatus.InReview:
                        return true;
                }
                return false;
            case DefaultActionPlanPreferenceStatus.InReview:
                switch (next)
                {
                    case DefaultActionPlanPreferenceStatus.Archived:
                        return true;
                }
                return false;
            case DefaultActionPlanPreferenceStatus.Resolved:
                switch (next)
                {
                    case DefaultActionPlanPreferenceStatus.Reopened:
                    case DefaultActionPlanPreferenceStatus.Archived:
                        return true;
                }
                return false;
            case DefaultActionPlanPreferenceStatus.Reopened:
                switch (next)
                {
                    case DefaultActionPlanPreferenceStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultActionPlanPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default action plan preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default action plan preference"} {Code}";
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

    public double EffectiveLoad()
    {
        return Math.Round(Amount - (double)Retries, 2);
    }

    public DefaultActionPlanPreference Copy()
    {
        return new DefaultActionPlanPreference(
            Code,
            Name,
            Source,
            Pinned,
            TenureMonths,
            Capacity,
            Attributes,
            Fte,
            Amount,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
