namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional assignment agreement is in its life cycle.</summary>
public enum RegionalAssignmentAgreementStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A regional assignment agreement. Its code is unique within a tenant.</summary>
public sealed class RegionalAssignmentAgreement(string code, string name, string salaryBand, string segment, string summary, int tenureMonths, List<string> tags, Dictionary<string, string> attributes, bool approved, RegionalAssignmentAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public string Segment { get; set; } = segment;
    public string Summary { get; set; } = summary;
    public int TenureMonths { get; set; } = tenureMonths;
    public List<string> Tags { get; set; } = tags;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Approved { get; set; } = approved;
    public RegionalAssignmentAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a regional assignment agreement in status idle.</summary>
    public static RegionalAssignmentAgreement NewRegionalAssignmentAgreement(string code, string name)
    {
        return new RegionalAssignmentAgreement(
            code,
            name,
            "",
            "",
            "",
            0,
            new List<string>(),
            new Dictionary<string, string>(),
            false,
            RegionalAssignmentAgreementStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAssignmentAgreementStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAssignmentAgreementStatus next)
    {
        switch (Status)
        {
            case RegionalAssignmentAgreementStatus.Idle:
                return next == RegionalAssignmentAgreementStatus.Warming || next == RegionalAssignmentAgreementStatus.Stopped;
            case RegionalAssignmentAgreementStatus.Warming:
                switch (next)
                {
                    case RegionalAssignmentAgreementStatus.Ready:
                    case RegionalAssignmentAgreementStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalAssignmentAgreementStatus.Ready:
                switch (next)
                {
                    case RegionalAssignmentAgreementStatus.Draining:
                        return true;
                }
                return false;
            case RegionalAssignmentAgreementStatus.Draining:
                switch (next)
                {
                    case RegionalAssignmentAgreementStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalAssignmentAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional assignment agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional assignment agreement"} {Code}";
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

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public RegionalAssignmentAgreement Copy()
    {
        return new RegionalAssignmentAgreement(
            Code,
            Name,
            SalaryBand,
            Segment,
            Summary,
            TenureMonths,
            Tags,
            Attributes,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
