namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default policy ack bundle is in its life cycle.</summary>
public enum DefaultPolicyAckBundleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default policy ack bundle. Its code is unique within a tenant.</summary>
public sealed class DefaultPolicyAckBundle(string code, string title, string name, double budget, double rate, string location, string summary, double score, double margin, DefaultPolicyAckBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public double Rate { get; set; } = rate;
    public string Location { get; set; } = location;
    public string Summary { get; set; } = summary;
    public double Score { get; set; } = score;
    public double Margin { get; set; } = margin;
    public DefaultPolicyAckBundleStatus Status { get; set; } = status;

    /// <summary>Starts a default policy ack bundle in status proposed.</summary>
    public static DefaultPolicyAckBundle NewDefaultPolicyAckBundle(string code, string title)
    {
        return new DefaultPolicyAckBundle(
            code,
            title,
            "",
            0.0,
            0.0,
            "",
            "",
            0.0,
            0.0,
            DefaultPolicyAckBundleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPolicyAckBundleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPolicyAckBundleStatus next)
    {
        switch (Status)
        {
            case DefaultPolicyAckBundleStatus.Proposed:
                switch (next)
                {
                    case DefaultPolicyAckBundleStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultPolicyAckBundleStatus.Approved:
                switch (next)
                {
                    case DefaultPolicyAckBundleStatus.Rejected:
                        return true;
                }
                return false;
            case DefaultPolicyAckBundleStatus.Rejected:
                switch (next)
                {
                    case DefaultPolicyAckBundleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPolicyAckBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default policy ack bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default policy ack bundle"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Rate * Budget, 2);
    }

    public DefaultPolicyAckBundle Copy()
    {
        return new DefaultPolicyAckBundle(Code, Title, Name, Budget, Rate, Location, Summary, Score, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
