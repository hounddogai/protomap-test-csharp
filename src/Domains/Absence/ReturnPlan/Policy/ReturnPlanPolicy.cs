namespace Acme.Workforce.Absence.ReturnPlan.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan policy is in its life cycle.</summary>
public enum ReturnPlanPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A return plan policy as the return plan team keeps it.</summary>
public sealed class ReturnPlanPolicy(string code, string label, string channel, double budget, int version, string location, string reviewer, Dictionary<string, string> attributes, List<string> tags, string unit, string salaryBand, ReturnPlanPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public int Version { get; set; } = version;
    public string Location { get; set; } = location;
    public string Reviewer { get; set; } = reviewer;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public List<string> Tags { get; set; } = tags;
    public string Unit { get; set; } = unit;
    public string SalaryBand { get; set; } = salaryBand;
    public ReturnPlanPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a return plan policy in status new.</summary>
    public static ReturnPlanPolicy NewReturnPlanPolicy(string code, string label)
    {
        return new ReturnPlanPolicy(
            code,
            label,
            "",
            0.0,
            0,
            "",
            "",
            new Dictionary<string, string>(),
            new List<string>(),
            "",
            "",
            ReturnPlanPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanPolicyStatus next)
    {
        switch (Status)
        {
            case ReturnPlanPolicyStatus.New:
                switch (next)
                {
                    case ReturnPlanPolicyStatus.Running:
                    case ReturnPlanPolicyStatus.Failed:
                        return true;
                }
                return false;
            case ReturnPlanPolicyStatus.Queued:
                switch (next)
                {
                    case ReturnPlanPolicyStatus.Running:
                    case ReturnPlanPolicyStatus.Done:
                        return true;
                }
                return false;
            case ReturnPlanPolicyStatus.Running:
                return next == ReturnPlanPolicyStatus.Done || next == ReturnPlanPolicyStatus.Failed;
            case ReturnPlanPolicyStatus.Done:
                switch (next)
                {
                    case ReturnPlanPolicyStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
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
        return Math.Round((double)Version + Budget, 2);
    }

    public ReturnPlanPolicy Copy()
    {
        return new ReturnPlanPolicy(
            Code,
            Label,
            Channel,
            Budget,
            Version,
            Location,
            Reviewer,
            Attributes,
            Tags,
            Unit,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
