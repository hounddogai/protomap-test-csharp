namespace Acme.Workforce.Talent.SkillProfile.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional skill profile summary is in its life cycle.</summary>
public enum RegionalSkillProfileSummaryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A regional skill profile summary. Its code is unique within a tenant.</summary>
public sealed class RegionalSkillProfileSummary(string code, string name, double amount, string phone, bool pinned, RegionalSkillProfileSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string Phone { get; set; } = phone;
    public bool Pinned { get; set; } = pinned;
    public RegionalSkillProfileSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a regional skill profile summary in status idle.</summary>
    public static RegionalSkillProfileSummary NewRegionalSkillProfileSummary(string code, string name)
    {
        return new RegionalSkillProfileSummary(code, name, 0.0, "", false, RegionalSkillProfileSummaryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSkillProfileSummaryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSkillProfileSummaryStatus next)
    {
        switch (Status)
        {
            case RegionalSkillProfileSummaryStatus.Idle:
                switch (next)
                {
                    case RegionalSkillProfileSummaryStatus.Warming:
                        return true;
                }
                return false;
            case RegionalSkillProfileSummaryStatus.Warming:
                return next == RegionalSkillProfileSummaryStatus.Draining;
            case RegionalSkillProfileSummaryStatus.Ready:
                switch (next)
                {
                    case RegionalSkillProfileSummaryStatus.Draining:
                        return true;
                }
                return false;
            case RegionalSkillProfileSummaryStatus.Draining:
                switch (next)
                {
                    case RegionalSkillProfileSummaryStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalSkillProfileSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional skill profile summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public RegionalSkillProfileSummary Copy()
    {
        return new RegionalSkillProfileSummary(Code, Name, Amount, Phone, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
