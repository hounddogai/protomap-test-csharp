namespace Acme.Workforce.Absence.ReturnPlan.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan preference is in its life cycle.</summary>
public enum ReturnPlanPreferenceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A return plan preference. Its code is unique within a tenant.</summary>
public sealed class ReturnPlanPreference(string code, string title, double weight, int quantity, string department, double minimum, double ratio, Dictionary<string, string> attributes, int priority, ReturnPlanPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public int Quantity { get; set; } = quantity;
    public string Department { get; set; } = department;
    public double Minimum { get; set; } = minimum;
    public double Ratio { get; set; } = ratio;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Priority { get; set; } = priority;
    public ReturnPlanPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a return plan preference in status idle.</summary>
    public static ReturnPlanPreference NewReturnPlanPreference(string code, string title)
    {
        return new ReturnPlanPreference(
            code,
            title,
            0.0,
            0,
            "",
            0.0,
            0.0,
            new Dictionary<string, string>(),
            0,
            ReturnPlanPreferenceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanPreferenceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanPreferenceStatus next)
    {
        switch (Status)
        {
            case ReturnPlanPreferenceStatus.Idle:
                return next == ReturnPlanPreferenceStatus.Stopped;
            case ReturnPlanPreferenceStatus.Warming:
                return next == ReturnPlanPreferenceStatus.Draining;
            case ReturnPlanPreferenceStatus.Ready:
                return next == ReturnPlanPreferenceStatus.Draining;
            case ReturnPlanPreferenceStatus.Draining:
                switch (next)
                {
                    case ReturnPlanPreferenceStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"return plan preference"} {Code}";
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

    public double NetValue()
    {
        return Math.Round(Ratio * (double)Priority, 2);
    }

    public ReturnPlanPreference Copy()
    {
        return new ReturnPlanPreference(
            Code,
            Title,
            Weight,
            Quantity,
            Department,
            Minimum,
            Ratio,
            Attributes,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
