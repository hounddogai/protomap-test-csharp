namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual leave request plan is in its life cycle.</summary>
public enum ManualLeaveRequestPlanStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a manual leave request plan and its current status.</summary>
public sealed class ManualLeaveRequestPlan(string code, string title, string locale, string jobCode, Dictionary<string, string> attributes, double amount, int windowDays, bool pinned, ManualLeaveRequestPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public string JobCode { get; set; } = jobCode;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Amount { get; set; } = amount;
    public int WindowDays { get; set; } = windowDays;
    public bool Pinned { get; set; } = pinned;
    public ManualLeaveRequestPlanStatus Status { get; set; } = status;

    /// <summary>Starts a manual leave request plan in status requested.</summary>
    public static ManualLeaveRequestPlan NewManualLeaveRequestPlan(string code, string title)
    {
        return new ManualLeaveRequestPlan(
            code,
            title,
            "",
            "",
            new Dictionary<string, string>(),
            0.0,
            0,
            false,
            ManualLeaveRequestPlanStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualLeaveRequestPlanStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualLeaveRequestPlanStatus next)
    {
        switch (Status)
        {
            case ManualLeaveRequestPlanStatus.Requested:
                switch (next)
                {
                    case ManualLeaveRequestPlanStatus.Revoked:
                        return true;
                }
                return false;
            case ManualLeaveRequestPlanStatus.Granted:
                switch (next)
                {
                    case ManualLeaveRequestPlanStatus.Expired:
                        return true;
                }
                return false;
            case ManualLeaveRequestPlanStatus.Revoked:
                switch (next)
                {
                    case ManualLeaveRequestPlanStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualLeaveRequestPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual leave request plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"manual leave request plan"} {Code}";
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

    public double EffectiveTotal()
    {
        return Math.Round(Amount - (double)WindowDays, 2);
    }

    public ManualLeaveRequestPlan Copy()
    {
        return new ManualLeaveRequestPlan(
            Code,
            Title,
            Locale,
            JobCode,
            Attributes,
            Amount,
            WindowDays,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
